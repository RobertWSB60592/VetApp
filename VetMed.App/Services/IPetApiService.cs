using VetMed.Shared.DTOs;

namespace VetMed.App.Services;

public interface IPetApiService
{
    Task<List<PetDto>> GetPetsAsync();
    Task<PetDto?> GetPetAsync(int id);
    Task<PetDto?> CreatePetAsync(CreatePetDto dto);
    Task<bool> DeletePetAsync(int id);
}
