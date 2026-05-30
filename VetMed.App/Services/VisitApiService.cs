using VetMed.Shared.DTOs;

namespace VetMed.App.Services;

public class VisitApiService : IVisitApiService
{
    private readonly ApiClient _api;

    public VisitApiService(ApiClient api)
    {
        _api = api;
    }

    public async Task<List<VisitDto>> GetVisitsAsync() =>
        await _api.GetAsync<List<VisitDto>>("api/visits") ?? [];

    public async Task<List<VisitDto>> GetVisitsByPetAsync(int petId) =>
        await _api.GetAsync<List<VisitDto>>($"api/visits?petId={petId}") ?? [];

    public async Task<List<SlotDto>> GetSlotsAsync(int doctorId, DateOnly date) =>
        await _api.GetAsync<List<SlotDto>>($"api/visits/slots?doctorId={doctorId}&date={date:yyyy-MM-dd}") ?? [];

    public Task<VisitDto?> CreateVisitAsync(CreateVisitDto dto) =>
        _api.PostAsync<VisitDto>("api/visits", dto);
}
