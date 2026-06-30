using VetMed.Shared.DTOs;

namespace VetMed.App.Services;

public interface IHealthApiService
{
    Task<List<VaccinationDto>> GetVaccinationsByPetAsync(int petId);
    Task<List<PrescriptionDto>> GetPrescriptionsByPetAsync(int petId);
    Task<List<VaccinationDto>> GetUpcomingVaccinationsAsync(int days = 30);
}
