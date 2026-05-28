using VetMed.Shared.DTOs;

namespace VetMed.App.Services;

public class PetApiService : IPetApiService
{
    private readonly ApiClient _api;

    public PetApiService(ApiClient api)
    {
        _api = api;
    }

    public async Task<List<PetDto>> GetPetsAsync() =>
        await _api.GetAsync<List<PetDto>>("api/pets") ?? [];

    public Task<PetDto?> GetPetAsync(int id) =>
        _api.GetAsync<PetDto>($"api/pets/{id}");

    public Task<PetDto?> CreatePetAsync(CreatePetDto dto) =>
        _api.PostAsync<PetDto>("api/pets", dto);

    public async Task<bool> DeletePetAsync(int id) =>
        await _api.DeleteAsync($"api/pets/{id}");
}
