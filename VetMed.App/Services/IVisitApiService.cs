using VetMed.Shared.DTOs;

namespace VetMed.App.Services;

public interface IVisitApiService
{
    Task<List<VisitDto>> GetVisitsAsync();
    Task<List<VisitDto>> GetVisitsByPetAsync(int petId);
    Task<VisitDto?> GetVisitAsync(int id);
    Task<List<SlotDto>> GetSlotsAsync(int doctorId, DateOnly date);
    Task<List<DayAvailabilityDto>> GetAvailabilityAsync(int doctorId, DateOnly from, int days);
    Task<VisitDto?> CreateVisitAsync(CreateVisitDto dto);
    Task<VisitDto?> RescheduleVisitAsync(int id, DateTime scheduledAtUtc);
    Task<VisitDto?> CancelVisitAsync(int id);
}
