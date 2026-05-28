using Microsoft.EntityFrameworkCore;
using VetMed.Infrastructure.Data;
using VetMed.Shared.Models;

namespace VetMed.Infrastructure.Repositories;

public class PetRepository : IPetRepository
{
    private readonly AppDbContext _db;

    public PetRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Pet>> GetByOwnerAsync(int ownerId, CancellationToken ct = default) =>
        await _db.Pets
            .AsNoTracking()
            .Where(p => p.OwnerId == ownerId)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);

    public async Task<Pet?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _db.Pets
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task AddAsync(Pet pet, CancellationToken ct = default) =>
        await _db.Pets.AddAsync(pet, ct);

    public void Update(Pet pet) =>
        _db.Pets.Update(pet);

    public void Delete(Pet pet) =>
        _db.Pets.Remove(pet);

    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await _db.SaveChangesAsync(ct);
}
