using VetMed.Infrastructure.Repositories;
using VetMed.Shared.DTOs;
using VetMed.Shared.Models;

namespace VetMed.Api.Services;

public sealed class VisitService : IVisitService
{
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

    public async Task<VisitDto?> CreateAsync(int ownerId, CreateVisitDto dto, CancellationToken ct = default)
    {
        var pet = await _pets.GetByIdAsync(dto.PetId, ct);
        if (pet is null || pet.OwnerId != ownerId)
            return null;

        var doctor = await _doctors.GetByIdAsync(dto.DoctorId, ct);
        if (doctor is null)
            return null;

        var visit = new Visit
        {
            ScheduledAt = dto.ScheduledAt,
            Type = dto.Type,
            Notes = dto.Notes,
            PetId = dto.PetId,
            DoctorId = dto.DoctorId
        };

        await _visits.AddAsync(visit, ct);
        await _visits.SaveChangesAsync(ct);

        visit.Pet = pet;
        visit.Doctor = doctor;

        return Map(visit);
    }

    private static VisitDto Map(Visit v) =>
        new(v.Id, v.ScheduledAt, v.Type, v.Status, v.Notes,
            v.PetId, v.Pet.Name, v.DoctorId, v.Doctor.FullName);
}
