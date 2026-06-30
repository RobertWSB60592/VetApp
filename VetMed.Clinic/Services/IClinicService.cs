using VetMed.Shared.Enums;
using VetMed.Shared.Models;

namespace VetMed.Clinic.Services;

public interface IClinicService
{
    Task<List<ClinicVisitVm>> GetPendingAsync(CancellationToken ct = default);
    Task<List<ClinicVisitVm>> GetAllAsync(VisitStatus? status, int? doctorId, CancellationToken ct = default);
    Task<int> CountPendingAsync(CancellationToken ct = default);
    Task<bool> ApproveAsync(int id, CancellationToken ct = default);
    Task<bool> CompleteAsync(int id, string? summary, CancellationToken ct = default);
    Task<bool> UpdateSummaryAsync(int id, string? summary, CancellationToken ct = default);
    Task<bool> RejectAsync(int id, string? reason, CancellationToken ct = default);
    Task<List<(int Id, string Label)>> GetPetOptionsAsync(CancellationToken ct = default);
    Task<List<Vaccination>> GetVaccinationsAsync(int petId, CancellationToken ct = default);
    Task<List<Prescription>> GetPrescriptionsAsync(int petId, CancellationToken ct = default);
    Task AddVaccinationAsync(Vaccination vaccination, CancellationToken ct = default);
    Task AddPrescriptionAsync(Prescription prescription, CancellationToken ct = default);
    Task<List<DoctorScheduleVm>> GetDoctorSchedulesAsync(CancellationToken ct = default);
    Task<List<(int Id, string Name)>> GetDoctorOptionsAsync(CancellationToken ct = default);
}
