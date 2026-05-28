using VetMed.Shared.Models;

namespace VetMed.Infrastructure.Repositories;

public interface IVisitRepository
{
    Task<IReadOnlyList<Visit>> GetByPetAsync(int petId, CancellationToken ct = default);
    Task<IReadOnlyList<Visit>> GetByOwnerAsync(int ownerId, CancellationToken ct = default);
    Task<Visit?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Visit visit, CancellationToken ct = default);
    void Update(Visit visit);
    Task SaveChangesAsync(CancellationToken ct = default);
}
