using Microsoft.EntityFrameworkCore;
using VetMed.Infrastructure.Data;
using VetMed.Shared.DTOs;

namespace VetMed.Api.Services;

public sealed class HealthService : IHealthService
{
    private readonly AppDbContext _db;

    public HealthService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<VaccinationDto>> GetVaccinationsByPetAsync(int petId, int ownerId, CancellationToken ct = default)
    {
        return await _db.Vaccinations
            .Where(v => v.PetId == petId && v.Pet.OwnerId == ownerId)
            .OrderByDescending(v => v.AdministeredOn)
            .Select(v => new VaccinationDto(v.Id, v.Name, v.AdministeredOn, v.NextDueOn, v.Notes, v.PetId, v.Pet.Name))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PrescriptionDto>> GetPrescriptionsByPetAsync(int petId, int ownerId, CancellationToken ct = default)
    {
        return await _db.Prescriptions
            .Where(p => p.PetId == petId && p.Pet.OwnerId == ownerId)
            .OrderByDescending(p => p.StartsOn)
            .Select(p => new PrescriptionDto(p.Id, p.Medication, p.Dosage, p.StartsOn, p.EndsOn, p.Notes, p.PetId, p.Pet.Name))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<VaccinationDto>> GetUpcomingVaccinationsAsync(int ownerId, int days, CancellationToken ct = default)
    {
        days = Math.Clamp(days, 1, 365);
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var until = today.AddDays(days);

        return await _db.Vaccinations
            .Where(v => v.Pet.OwnerId == ownerId
                && !v.Pet.IsArchived
                && v.NextDueOn != null
                && v.NextDueOn >= today
                && v.NextDueOn <= until)
            .OrderBy(v => v.NextDueOn)
            .Select(v => new VaccinationDto(v.Id, v.Name, v.AdministeredOn, v.NextDueOn, v.Notes, v.PetId, v.Pet.Name))
            .ToListAsync(ct);
    }
}
