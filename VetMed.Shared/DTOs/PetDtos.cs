using VetMed.Shared.Enums;

namespace VetMed.Shared.DTOs;

public record PetDto(
    int Id,
    string Name,
    string Species,
    string? Breed,
    decimal? WeightKg,
    DateOnly Born,
    string? ImageUrl,
    PetSex Sex,
    bool Sterilized,
    string? MicrochipNumber,
    string? Color,
    string? Notes);

public record CreatePetDto(
    string Name,
    string Species,
    string? Breed,
    decimal? WeightKg,
    DateOnly Born,
    string? ImageUrl,
    PetSex Sex,
    bool Sterilized,
    string? MicrochipNumber,
    string? Color,
    string? Notes);

public record UpdatePetDto(
    string Name,
    string Species,
    string? Breed,
    decimal? WeightKg,
    DateOnly Born,
    string? ImageUrl,
    PetSex Sex,
    bool Sterilized,
    string? MicrochipNumber,
    string? Color,
    string? Notes);
