using Microsoft.EntityFrameworkCore;
using VetMed.Infrastructure.Data;
using VetMed.Shared.Models;

namespace VetMed.Infrastructure.Repositories;

public class DoctorRepository : IDoctorRepository
{
    private readonly AppDbContext _db;

    public DoctorRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Doctor>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Doctors
            .AsNoTracking()
            .OrderBy(d => d.FullName)
            .ToListAsync(ct);

    public async Task<Doctor?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _db.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, ct);
}
