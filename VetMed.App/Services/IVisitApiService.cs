using VetMed.Shared.DTOs;

namespace VetMed.App.Services;

public interface IVisitApiService
{
    Task<List<VisitDto>> GetVisitsAsync();
    Task<List<VisitDto>> GetVisitsByPetAsync(int petId);
    Task<List<SlotDto>> GetSlotsAsync(int doctorId, DateOnly date);
    Task<VisitDto?> CreateVisitAsync(CreateVisitDto dto);
}
