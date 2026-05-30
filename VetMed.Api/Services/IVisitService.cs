using VetMed.Shared.DTOs;

namespace VetMed.Api.Services;

public enum BookingOutcome
{
    Created,
    PetOrDoctorNotFound,
    SlotUnavailable
}

public readonly record struct BookingResult(BookingOutcome Outcome, VisitDto? Visit);

public interface IVisitService
{
    Task<IReadOnlyList<VisitDto>> GetByOwnerAsync(int ownerId, CancellationToken ct = default);
    Task<IReadOnlyList<VisitDto>> GetByPetAsync(int petId, int ownerId, CancellationToken ct = default);
    Task<IReadOnlyList<SlotDto>> GetAvailableSlotsAsync(int doctorId, DateOnly date, CancellationToken ct = default);
    Task<BookingResult> CreateAsync(int ownerId, CreateVisitDto dto, CancellationToken ct = default);
}
