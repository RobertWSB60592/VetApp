using Microsoft.EntityFrameworkCore;
using VetMed.Infrastructure.Data;
using VetMed.Shared.Enums;
using VetMed.Shared.Models;

namespace VetMed.Infrastructure.Repositories;

public class VisitRepository : IVisitRepository
{
    private readonly AppDbContext _db;

    public VisitRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Visit>> GetByPetAsync(int petId, CancellationToken ct = default) =>
        await _db.Visits
            .AsNoTracking()
            .Include(v => v.Doctor)
            .Include(v => v.Pet)
            .Where(v => v.PetId == petId)
            .OrderByDescending(v => v.ScheduledAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Visit>> GetByOwnerAsync(int ownerId, CancellationToken ct = default) =>
        await _db.Visits
            .AsNoTracking()
            .Include(v => v.Doctor)
            .Include(v => v.Pet)
            .Where(v => v.Pet.OwnerId == ownerId)
            .OrderByDescending(v => v.ScheduledAt)
            .ToListAsync(ct);

    public async Task<Visit?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _db.Visits
            .AsNoTracking()
            .Include(v => v.Doctor)
            .Include(v => v.Pet)
            .FirstOrDefaultAsync(v => v.Id == id, ct);

    public async Task<IReadOnlyList<Visit>> GetActiveByDoctorOnDateAsync(int doctorId, DateTime dayStartUtc, DateTime dayEndUtc, CancellationToken ct = default) =>
        await _db.Visits
            .AsNoTracking()
            .Where(v => v.DoctorId == doctorId
                        && v.Status != VisitStatus.Odwolana
                        && v.ScheduledAt >= dayStartUtc
                        && v.ScheduledAt < dayEndUtc)
            .ToListAsync(ct);

    public async Task<bool> SlotTakenAsync(int doctorId, DateTime scheduledAtUtc, int? excludeVisitId = null, CancellationToken ct = default) =>
        await _db.Visits
            .AsNoTracking()
            .AnyAsync(v => v.DoctorId == doctorId
                           && v.Status != VisitStatus.Odwolana
                           && v.ScheduledAt == scheduledAtUtc
                           && (excludeVisitId == null || v.Id != excludeVisitId), ct);

    public async Task AddAsync(Visit visit, CancellationToken ct = default) =>
        await _db.Visits.AddAsync(visit, ct);

    public void Update(Visit visit) =>
        _db.Visits.Update(visit);

    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await _db.SaveChangesAsync(ct);
}
