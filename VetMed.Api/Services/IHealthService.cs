using VetMed.Shared.DTOs;

namespace VetMed.Api.Services;

public interface IHealthService
{
    Task<IReadOnlyList<VaccinationDto>> GetVaccinationsByPetAsync(int petId, int ownerId, CancellationToken ct = default);
    Task<IReadOnlyList<PrescriptionDto>> GetPrescriptionsByPetAsync(int petId, int ownerId, CancellationToken ct = default);
    Task<IReadOnlyList<VaccinationDto>> GetUpcomingVaccinationsAsync(int ownerId, int days, CancellationToken ct = default);
}
