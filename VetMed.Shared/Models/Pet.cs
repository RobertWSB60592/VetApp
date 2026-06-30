using VetMed.Shared.Enums;

namespace VetMed.Shared.Models;

/// <summary>Zwierzę (pupil) należące do właściciela.</summary>
public class Pet
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public decimal? WeightKg { get; set; }
    public DateOnly Born { get; set; }
    public string? ImageUrl { get; set; }

    public PetSex Sex { get; set; } = PetSex.Nieznana;
    public bool Sterilized { get; set; }
    public string? MicrochipNumber { get; set; }
    public string? Color { get; set; }
    public string? Notes { get; set; }
    public bool IsArchived { get; set; }

    public int OwnerId { get; set; }
    public Owner Owner { get; set; } = null!;

    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
    public ICollection<Vaccination> Vaccinations { get; set; } = new List<Vaccination>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
