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

    public Task<VisitDto?> GetVisitAsync(int id) =>
        _api.GetAsync<VisitDto>($"api/visits/{id}");

    public async Task<List<SlotDto>> GetSlotsAsync(int doctorId, DateOnly date) =>
        await _api.GetAsync<List<SlotDto>>($"api/visits/slots?doctorId={doctorId}&date={date:yyyy-MM-dd}") ?? [];

    public async Task<List<DayAvailabilityDto>> GetAvailabilityAsync(int doctorId, DateOnly from, int days) =>
        await _api.GetAsync<List<DayAvailabilityDto>>($"api/visits/availability?doctorId={doctorId}&from={from:yyyy-MM-dd}&days={days}") ?? [];

    public Task<VisitDto?> CreateVisitAsync(CreateVisitDto dto) =>
        _api.PostAsync<VisitDto>("api/visits", dto);

    public Task<VisitDto?> RescheduleVisitAsync(int id, DateTime scheduledAtUtc) =>
        _api.PatchAsync<VisitDto>($"api/visits/{id}", new RescheduleVisitDto(scheduledAtUtc));

    public Task<VisitDto?> CancelVisitAsync(int id) =>
        _api.PostAsync<VisitDto>($"api/visits/{id}/cancel", new { });
}
