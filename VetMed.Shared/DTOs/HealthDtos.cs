namespace VetMed.Shared.DTOs;

public record VaccinationDto(
    int Id,
    string Name,
    DateOnly AdministeredOn,
    DateOnly? NextDueOn,
    string? Notes,
    int PetId,
    string PetName);

public record CreateVaccinationDto(
    string Name,
    DateOnly AdministeredOn,
    DateOnly? NextDueOn,
    string? Notes,
    int PetId);

public record PrescriptionDto(
    int Id,
    string Medication,
    string? Dosage,
    DateOnly StartsOn,
    DateOnly? EndsOn,
    string? Notes,
    int PetId,
    string PetName);

public record CreatePrescriptionDto(
    string Medication,
    string? Dosage,
    DateOnly StartsOn,
    DateOnly? EndsOn,
    string? Notes,
    int PetId);
