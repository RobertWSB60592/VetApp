using VetMed.Shared.Enums;

namespace VetMed.Shared.DTOs;

public record VisitDto(
    int Id,
    DateTime ScheduledAt,
    VisitType Type,
    VisitStatus Status,
    string? Notes,
    int PetId,
    string PetName,
    int DoctorId,
    string DoctorName);

public record CreateVisitDto(
    DateTime ScheduledAt,
    VisitType Type,
    int PetId,
    int DoctorId,
    string? Notes);

public record RescheduleVisitDto(DateTime ScheduledAt);

public record DoctorDto(int Id, string FullName, string Specialization, bool IsAvailable);

public record SlotDto(string Time, bool Available);

public record DayAvailabilityDto(DateOnly Date, bool HasFreeSlots);
