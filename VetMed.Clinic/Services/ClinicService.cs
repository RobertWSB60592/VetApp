using Microsoft.EntityFrameworkCore;
using VetMed.Infrastructure.Data;
using VetMed.Shared.Enums;
using VetMed.Shared.Models;

namespace VetMed.Clinic.Services;

public record ClinicVisitVm(
    int Id,
    DateTime ScheduledAt,
    VisitType Type,
    VisitStatus Status,
    string? Notes,
    string? RejectionReason,
    string? DoctorSummary,
    string PetName,
    string PetSpecies,
    string OwnerName,
    string? OwnerPhone,
    string DoctorName);

public record DoctorScheduleVm(
    int DoctorId,
    string DoctorName,
    string Specialization,
    bool IsAvailable,
    IReadOnlyList<ScheduleEntryVm> Schedule);

public record ScheduleEntryVm(DayOfWeek Day, TimeOnly Start, TimeOnly End);

public sealed class ClinicService : IClinicService
{
    private readonly AppDbContext _db;

    public ClinicService(AppDbContext db) => _db = db;

    public async Task<List<ClinicVisitVm>> GetPendingAsync(CancellationToken ct = default) =>
        await Project(_db.Visits
                .Where(v => v.Status == VisitStatus.Oczekujaca)
                .OrderBy(v => v.ScheduledAt))
            .ToListAsync(ct);

    public async Task<List<ClinicVisitVm>> GetAllAsync(VisitStatus? status, int? doctorId, CancellationToken ct = default)
    {
        var q = _db.Visits.AsQueryable();
        if (status.HasValue)
            q = q.Where(v => v.Status == status.Value);
        if (doctorId.HasValue)
            q = q.Where(v => v.DoctorId == doctorId.Value);

        return await Project(q.OrderByDescending(v => v.ScheduledAt))
            .ToListAsync(ct);
    }

    public async Task<int> CountPendingAsync(CancellationToken ct = default) =>
        await _db.Visits.CountAsync(v => v.Status == VisitStatus.Oczekujaca, ct);

    public async Task<bool> ApproveAsync(int id, CancellationToken ct = default)
    {
        var visit = await _db.Visits.FirstOrDefaultAsync(v => v.Id == id, ct);
        if (visit is null || visit.Status != VisitStatus.Oczekujaca)
            return false;

        visit.Status = VisitStatus.Potwierdzona;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> CompleteAsync(int id, string? summary, CancellationToken ct = default)
    {
        var visit = await _db.Visits.FirstOrDefaultAsync(v => v.Id == id, ct);
        if (visit is null || visit.Status is not (VisitStatus.Potwierdzona or VisitStatus.Zaplanowana))
            return false;

        visit.Status = VisitStatus.Zakonczona;
        visit.DoctorSummary = string.IsNullOrWhiteSpace(summary) ? null : summary.Trim();
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> UpdateSummaryAsync(int id, string? summary, CancellationToken ct = default)
    {
        var visit = await _db.Visits.FirstOrDefaultAsync(v => v.Id == id, ct);
        if (visit is null || visit.Status != VisitStatus.Zakonczona)
            return false;

        visit.DoctorSummary = string.IsNullOrWhiteSpace(summary) ? null : summary.Trim();
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> RejectAsync(int id, string? reason, CancellationToken ct = default)
    {
        var visit = await _db.Visits.FirstOrDefaultAsync(v => v.Id == id, ct);
        if (visit is null || visit.Status != VisitStatus.Oczekujaca)
            return false;

        visit.Status = VisitStatus.Odwolana;
        visit.RejectionReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<(int Id, string Label)>> GetPetOptionsAsync(CancellationToken ct = default) =>
        (await _db.Pets
            .Where(p => !p.IsArchived)
            .OrderBy(p => p.Name)
            .Select(p => new { p.Id, p.Name, p.Species, Owner = p.Owner.FullName })
            .ToListAsync(ct))
        .Select(p => (p.Id, $"{p.Name} ({p.Species}) — {p.Owner}"))
        .ToList();

    public async Task<List<Vaccination>> GetVaccinationsAsync(int petId, CancellationToken ct = default) =>
        await _db.Vaccinations
            .Where(v => v.PetId == petId)
            .OrderByDescending(v => v.AdministeredOn)
            .ToListAsync(ct);

    public async Task<List<Prescription>> GetPrescriptionsAsync(int petId, CancellationToken ct = default) =>
        await _db.Prescriptions
            .Where(p => p.PetId == petId)
            .OrderByDescending(p => p.StartsOn)
            .ToListAsync(ct);

    public async Task AddVaccinationAsync(Vaccination vaccination, CancellationToken ct = default)
    {
        _db.Vaccinations.Add(vaccination);
        await _db.SaveChangesAsync(ct);
    }

    public async Task AddPrescriptionAsync(Prescription prescription, CancellationToken ct = default)
    {
        _db.Prescriptions.Add(prescription);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<DoctorScheduleVm>> GetDoctorSchedulesAsync(CancellationToken ct = default) =>
        await _db.Doctors
            .OrderBy(d => d.FullName)
            .Select(d => new DoctorScheduleVm(
                d.Id,
                d.FullName,
                d.Specialization,
                d.IsAvailable,
                d.Schedules
                    .OrderBy(s => s.DayOfWeek)
                    .ThenBy(s => s.StartTime)
                    .Select(s => new ScheduleEntryVm(s.DayOfWeek, s.StartTime, s.EndTime))
                    .ToList()))
            .ToListAsync(ct);

    public async Task<List<(int Id, string Name)>> GetDoctorOptionsAsync(CancellationToken ct = default) =>
        (await _db.Doctors
            .OrderBy(d => d.FullName)
            .Select(d => new { d.Id, d.FullName })
            .ToListAsync(ct))
            .Select(d => (d.Id, d.FullName))
            .ToList();

    private static IQueryable<ClinicVisitVm> Project(IQueryable<Visit> q) =>
        q.Select(v => new ClinicVisitVm(
            v.Id,
            v.ScheduledAt,
            v.Type,
            v.Status,
            v.Notes,
            v.RejectionReason,
            v.DoctorSummary,
            v.Pet.Name,
            v.Pet.Species,
            v.Pet.Owner.FullName,
            v.Pet.Owner.Phone,
            v.Doctor.FullName));
}
