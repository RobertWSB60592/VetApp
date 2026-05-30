using VetMed.Shared.Models;

namespace VetMed.Infrastructure.Repositories;

public interface IDoctorRepository
{
    Task<IReadOnlyList<Doctor>> GetAllAsync(CancellationToken ct = default);
    Task<Doctor?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<DoctorSchedule>> GetSchedulesAsync(int doctorId, DayOfWeek dayOfWeek, CancellationToken ct = default);
}
