using VetMed.Infrastructure.Repositories;
using VetMed.Shared.DTOs;
using VetMed.Shared.Enums;
using VetMed.Shared.Models;

namespace VetMed.Api.Services;

public sealed class VisitService : IVisitService
{
    private static readonly TimeSpan SlotLength = TimeSpan.FromMinutes(30);

    private readonly IVisitRepository _visits;
    private readonly IPetRepository _pets;
    private readonly IDoctorRepository _doctors;

    public VisitService(IVisitRepository visits, IPetRepository pets, IDoctorRepository doctors)
    {
        _visits = visits;
        _pets = pets;
        _doctors = doctors;
    }

    public async Task<IReadOnlyList<VisitDto>> GetByOwnerAsync(int ownerId, CancellationToken ct = default)
    {
        var visits = await _visits.GetByOwnerAsync(ownerId, ct);
        return visits.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<VisitDto>> GetByPetAsync(int petId, int ownerId, CancellationToken ct = default)
    {
        var pet = await _pets.GetByIdAsync(petId, ct);
        if (pet is null || pet.OwnerId != ownerId)
            return [];

        var visits = await _visits.GetByPetAsync(petId, ct);
        return visits.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<DayAvailabilityDto>> GetAvailabilityAsync(int doctorId, DateOnly from, int days, CancellationToken ct = default)
    {
        days = Math.Clamp(days, 1, 31);
        var rangeStartUtc = DateTime.SpecifyKind(from.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var visits = await _visits.GetActiveByDoctorOnDateAsync(doctorId, rangeStartUtc, rangeStartUtc.AddDays(days), ct);
        var now = DateTime.UtcNow;

        var scheduleCache = new Dictionary<DayOfWeek, IReadOnlyList<DoctorSchedule>>();
        var result = new List<DayAvailabilityDto>(days);

        for (var i = 0; i < days; i++)
        {
            var date = from.AddDays(i);
            if (!scheduleCache.TryGetValue(date.DayOfWeek, out var schedules))
            {
                schedules = await _doctors.GetSchedulesAsync(doctorId, date.DayOfWeek, ct);
                scheduleCache[date.DayOfWeek] = schedules;
            }

            var takenTimes = visits
                .Where(v => DateOnly.FromDateTime(v.ScheduledAt) == date)
                .Select(v => TimeOnly.FromDateTime(v.ScheduledAt))
                .ToHashSet();

            var hasFree = false;
            foreach (var schedule in schedules)
            {
                for (var t = schedule.StartTime; t < schedule.EndTime; t = t.Add(SlotLength))
                {
                    var slotUtc = DateTime.SpecifyKind(date.ToDateTime(t), DateTimeKind.Utc);
                    if (!takenTimes.Contains(t) && slotUtc > now)
                    {
                        hasFree = true;
                        break;
                    }
                }
                if (hasFree)
                    break;
            }

            result.Add(new DayAvailabilityDto(date, hasFree));
        }

        return result;
    }

    public async Task<VisitDto?> GetByIdAsync(int visitId, int ownerId, CancellationToken ct = default)
    {
        var visit = await _visits.GetByIdAsync(visitId, ct);
        if (visit is null || visit.Pet.OwnerId != ownerId)
            return null;

        return Map(visit);
    }

    public async Task<IReadOnlyList<SlotDto>> GetAvailableSlotsAsync(int doctorId, DateOnly date, CancellationToken ct = default)
    {
        var schedules = await _doctors.GetSchedulesAsync(doctorId, date.DayOfWeek, ct);
        if (schedules.Count == 0)
            return [];

        var dayStartUtc = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var taken = await _visits.GetActiveByDoctorOnDateAsync(doctorId, dayStartUtc, dayStartUtc.AddDays(1), ct);
        var takenTimes = taken.Select(v => TimeOnly.FromDateTime(v.ScheduledAt)).ToHashSet();
        var now = DateTime.UtcNow;

        var slots = new List<SlotDto>();
        foreach (var schedule in schedules.OrderBy(s => s.StartTime))
        {
            for (var t = schedule.StartTime; t < schedule.EndTime; t = t.Add(SlotLength))
            {
                var slotUtc = DateTime.SpecifyKind(date.ToDateTime(t), DateTimeKind.Utc);
                var available = !takenTimes.Contains(t) && slotUtc > now;
                slots.Add(new SlotDto(t.ToString("HH:mm"), available));
            }
        }

        return slots;
    }

    public async Task<BookingResult> CreateAsync(int ownerId, CreateVisitDto dto, CancellationToken ct = default)
    {
        var pet = await _pets.GetByIdAsync(dto.PetId, ct);
        if (pet is null || pet.OwnerId != ownerId)
            return new BookingResult(BookingOutcome.PetOrDoctorNotFound, null);

        var doctor = await _doctors.GetByIdAsync(dto.DoctorId, ct);
        if (doctor is null)
            return new BookingResult(BookingOutcome.PetOrDoctorNotFound, null);

        var scheduledUtc = dto.ScheduledAt.ToUniversalTime();
        if (!await IsWithinScheduleAsync(dto.DoctorId, scheduledUtc, ct)
            || await _visits.SlotTakenAsync(dto.DoctorId, scheduledUtc, ct: ct))
            return new BookingResult(BookingOutcome.SlotUnavailable, null);

        var visit = new Visit
        {
            ScheduledAt = scheduledUtc,
            Type = dto.Type,
            Status = VisitStatus.Oczekujaca,
            Notes = dto.Notes,
            PetId = dto.PetId,
            DoctorId = dto.DoctorId
        };

        await _visits.AddAsync(visit, ct);
        await _visits.SaveChangesAsync(ct);

        visit.Pet = pet;
        visit.Doctor = doctor;

        return new BookingResult(BookingOutcome.Created, Map(visit));
    }

    public async Task<VisitMutationResult> RescheduleAsync(int ownerId, int visitId, RescheduleVisitDto dto, CancellationToken ct = default)
    {
        var visit = await _visits.GetTrackedByIdAsync(visitId, ct);
        if (visit is null || visit.Pet.OwnerId != ownerId)
            return new VisitMutationResult(VisitMutationOutcome.NotFound, null);

        if (!IsEditable(visit))
            return new VisitMutationResult(VisitMutationOutcome.NotEditable, null);

        var scheduledUtc = dto.ScheduledAt.ToUniversalTime();
        if (scheduledUtc <= DateTime.UtcNow
            || !await IsWithinScheduleAsync(visit.DoctorId, scheduledUtc, ct)
            || await _visits.SlotTakenAsync(visit.DoctorId, scheduledUtc, visitId, ct))
            return new VisitMutationResult(VisitMutationOutcome.SlotUnavailable, null);

        visit.ScheduledAt = scheduledUtc;
        await _visits.SaveChangesAsync(ct);

        return new VisitMutationResult(VisitMutationOutcome.Ok, Map(visit));
    }

    public async Task<VisitMutationResult> CancelAsync(int ownerId, int visitId, CancellationToken ct = default)
    {
        var visit = await _visits.GetTrackedByIdAsync(visitId, ct);
        if (visit is null || visit.Pet.OwnerId != ownerId)
            return new VisitMutationResult(VisitMutationOutcome.NotFound, null);

        if (!IsEditable(visit))
            return new VisitMutationResult(VisitMutationOutcome.NotEditable, null);

        visit.Status = VisitStatus.Odwolana;
        await _visits.SaveChangesAsync(ct);

        return new VisitMutationResult(VisitMutationOutcome.Ok, Map(visit));
    }

    private static bool IsEditable(Visit visit) =>
        visit.Status != VisitStatus.Odwolana
        && visit.Status != VisitStatus.Zakonczona
        && visit.ScheduledAt > DateTime.UtcNow;

    private async Task<bool> IsWithinScheduleAsync(int doctorId, DateTime scheduledUtc, CancellationToken ct)
    {
        var schedules = await _doctors.GetSchedulesAsync(doctorId, scheduledUtc.DayOfWeek, ct);
        if (schedules.Count == 0)
            return false;

        var time = TimeOnly.FromDateTime(scheduledUtc);
        var alignedToGrid = (time.ToTimeSpan() - schedules.Min(s => s.StartTime).ToTimeSpan()).Ticks % SlotLength.Ticks == 0;
        return alignedToGrid && schedules.Any(s => time >= s.StartTime && time < s.EndTime);
    }

    private static VisitDto Map(Visit v) =>
        new(v.Id, v.ScheduledAt, v.Type, v.Status, v.Notes,
            v.PetId, v.Pet.Name, v.DoctorId, v.Doctor.FullName, v.RejectionReason);
}
