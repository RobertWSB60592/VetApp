using VetMed.Shared.Models;

namespace VetMed.Infrastructure.Repositories;

public interface IPetRepository
{
    Task<IReadOnlyList<Pet>> GetByOwnerAsync(int ownerId, CancellationToken ct = default);
    Task<Pet?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Pet pet, CancellationToken ct = default);
    void Update(Pet pet);
    void Delete(Pet pet);
    Task SaveChangesAsync(CancellationToken ct = default);
}
