using VetMed.Shared.DTOs;

namespace VetMed.App.Services;

public class HealthApiService : IHealthApiService
{
    private readonly ApiClient _api;

    public HealthApiService(ApiClient api) => _api = api;

    public async Task<List<VaccinationDto>> GetVaccinationsByPetAsync(int petId) =>
        await _api.GetAsync<List<VaccinationDto>>($"api/health/vaccinations/pet/{petId}") ?? [];

    public async Task<List<PrescriptionDto>> GetPrescriptionsByPetAsync(int petId) =>
        await _api.GetAsync<List<PrescriptionDto>>($"api/health/prescriptions/pet/{petId}") ?? [];

    public async Task<List<VaccinationDto>> GetUpcomingVaccinationsAsync(int days = 30) =>
        await _api.GetAsync<List<VaccinationDto>>($"api/health/vaccinations/upcoming?days={days}") ?? [];
}
