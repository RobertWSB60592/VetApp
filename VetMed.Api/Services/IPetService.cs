using VetMed.Shared.DTOs;

namespace VetMed.Api.Services;

public interface IPetService
{
    Task<IReadOnlyList<PetDto>> GetByOwnerAsync(int ownerId, CancellationToken ct = default);
    Task<PetDto?> GetByIdAsync(int id, int ownerId, CancellationToken ct = default);
    Task<PetDto> CreateAsync(int ownerId, CreatePetDto dto, CancellationToken ct = default);
    Task<PetDto?> UpdateAsync(int id, int ownerId, UpdatePetDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, int ownerId, CancellationToken ct = default);
}
