using VetMed.Shared.Enums;

namespace VetMed.Shared.DTOs;

public record PetDto(
    int Id,
    string Name,
    Species Species,
    string? Breed,
    decimal? WeightKg,
    DateOnly Born,
    string? ImageUrl);

public record CreatePetDto(
    string Name,
    Species Species,
    string? Breed,
    decimal? WeightKg,
    DateOnly Born,
    string? ImageUrl);

public record UpdatePetDto(
    string Name,
    Species Species,
    string? Breed,
    decimal? WeightKg,
    string? ImageUrl);
