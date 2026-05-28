using VetMed.Infrastructure.Repositories;
using VetMed.Shared.DTOs;
using VetMed.Shared.Models;

namespace VetMed.Api.Services;

public sealed class PetService : IPetService
{
    private readonly IPetRepository _repo;

    public PetService(IPetRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<PetDto>> GetByOwnerAsync(int ownerId, CancellationToken ct = default)
    {
        var pets = await _repo.GetByOwnerAsync(ownerId, ct);
        return pets.Select(Map).ToList();
    }

    public async Task<PetDto?> GetByIdAsync(int id, int ownerId, CancellationToken ct = default)
    {
        var pet = await _repo.GetByIdAsync(id, ct);
        if (pet is null || pet.OwnerId != ownerId)
            return null;
        return Map(pet);
    }

    public async Task<PetDto> CreateAsync(int ownerId, CreatePetDto dto, CancellationToken ct = default)
    {
        var pet = new Pet
        {
            Name = dto.Name,
            Species = dto.Species,
            Breed = dto.Breed,
            WeightKg = dto.WeightKg,
            Born = dto.Born,
            ImageUrl = dto.ImageUrl,
            OwnerId = ownerId
        };

        await _repo.AddAsync(pet, ct);
        await _repo.SaveChangesAsync(ct);
        return Map(pet);
    }

    public async Task<PetDto?> UpdateAsync(int id, int ownerId, UpdatePetDto dto, CancellationToken ct = default)
    {
        var pet = await _repo.GetByIdAsync(id, ct);
        if (pet is null || pet.OwnerId != ownerId)
            return null;

        pet.Name = dto.Name;
        pet.Species = dto.Species;
        pet.Breed = dto.Breed;
        pet.WeightKg = dto.WeightKg;
        pet.ImageUrl = dto.ImageUrl;

        _repo.Update(pet);
        await _repo.SaveChangesAsync(ct);
        return Map(pet);
    }

    public async Task<bool> DeleteAsync(int id, int ownerId, CancellationToken ct = default)
    {
        var pet = await _repo.GetByIdAsync(id, ct);
        if (pet is null || pet.OwnerId != ownerId)
            return false;

        _repo.Delete(pet);
        await _repo.SaveChangesAsync(ct);
        return true;
    }

    private static PetDto Map(Pet p) =>
        new(p.Id, p.Name, p.Species, p.Breed, p.WeightKg, p.Born, p.ImageUrl);
}
