using Microsoft.EntityFrameworkCore;
using VetMed.Shared.Enums;
using VetMed.Shared.Models;

namespace VetMed.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, Func<string, string> hashPassword, CancellationToken ct = default)
    {
        if (await db.Doctors.AnyAsync(ct) || await db.Owners.AnyAsync(ct))
            return;

        var doctors = new List<Doctor>
        {
            new() { FullName = "dr Kowalska", Specialization = "Choroby wewnętrzne", IsAvailable = true },
            new() { FullName = "dr Nowak",    Specialization = "Chirurgia",           IsAvailable = true },
            new() { FullName = "dr Wiśniewska", Specialization = "Dermatologia",      IsAvailable = true }
        };
        db.Doctors.AddRange(doctors);

        var owner = new Owner
        {
            FullName     = "Robert Demo",
            Email        = "demo@vetmed.pl",
            PasswordHash = "NOPASSWORD",
            CreatedAt    = DateTime.UtcNow
        };

        var testOwner = new Owner
        {
            FullName     = "Konto Testowe",
            Email        = "test@vetmed.pl",
            PasswordHash = "NOPASSWORD",
            CreatedAt    = DateTime.UtcNow
        };

        var adminOwner = new Owner
        {
            FullName     = "Administrator",
            Email        = "admin@vetmed.pl",
            PasswordHash = hashPassword("admin123"),
            CreatedAt    = DateTime.UtcNow
        };

        db.Owners.AddRange(owner, testOwner, adminOwner);

        var pets = new List<Pet>
        {
            new() { Name = "Ozzy",    Species = Species.Kot,  Breed = "Brytyjski krótkowłosy", WeightKg = 4.8m,  Born = new DateOnly(2019, 3, 12), Owner = owner },
            new() { Name = "Ryszard", Species = Species.Pies, Breed = "Labrador retriever",     WeightKg = 28.5m, Born = new DateOnly(2018, 7, 4),  Owner = owner },
            new() { Name = "Atos",    Species = Species.Kot,  Breed = "Maine Coon",             WeightKg = 6.1m,  Born = new DateOnly(2021, 1, 20), Owner = owner },
            new() { Name = "Freddie", Species = Species.Pies, Breed = "Beagle",                 WeightKg = 11.2m, Born = new DateOnly(2020, 9, 8),  Owner = owner }
        };
        db.Pets.AddRange(pets);

        await db.SaveChangesAsync(ct);
    }
}
