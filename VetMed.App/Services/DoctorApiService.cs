using VetMed.Shared.DTOs;

namespace VetMed.App.Services;

public class DoctorApiService : IDoctorApiService
{
    private readonly ApiClient _api;

    public DoctorApiService(ApiClient api)
    {
        _api = api;
    }

    public async Task<List<DoctorDto>> GetDoctorsAsync() =>
        await _api.GetAsync<List<DoctorDto>>("api/doctors") ?? [];
}
