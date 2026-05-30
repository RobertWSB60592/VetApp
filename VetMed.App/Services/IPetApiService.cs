using VetMed.Shared.DTOs;

namespace VetMed.App.Services;

public interface IPetApiService
{
    Task<List<PetDto>> GetPetsAsync();
    Task<PetDto?> GetPetAsync(int id);
    Task<PetDto?> CreatePetAsync(CreatePetDto dto);
    Task<PetDto?> UpdatePetAsync(int id, UpdatePetDto dto);
    Task<bool> DeletePetAsync(int id);
}
