using VetMed.Shared.DTOs;

namespace VetMed.App.Services;

public interface IVisitApiService
{
    Task<List<VisitDto>> GetVisitsAsync();
    Task<List<VisitDto>> GetVisitsByPetAsync(int petId);
    Task<VisitDto?> CreateVisitAsync(CreateVisitDto dto);
}
