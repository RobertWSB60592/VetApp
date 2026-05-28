using VetMed.Shared.DTOs;

namespace VetMed.Api.Services;

public interface IVisitService
{
    Task<IReadOnlyList<VisitDto>> GetByOwnerAsync(int ownerId, CancellationToken ct = default);
    Task<IReadOnlyList<VisitDto>> GetByPetAsync(int petId, int ownerId, CancellationToken ct = default);
    Task<VisitDto?> CreateAsync(int ownerId, CreateVisitDto dto, CancellationToken ct = default);
}
