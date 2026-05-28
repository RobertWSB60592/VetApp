using VetMed.Shared.Enums;

namespace VetMed.Shared.Models;

/// <summary>Zwierzę (pupil) należące do właściciela.</summary>
public class Pet
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Species Species { get; set; }
    public string? Breed { get; set; }
    public decimal? WeightKg { get; set; }
    public DateOnly Born { get; set; }
    public string? ImageUrl { get; set; }

    public int OwnerId { get; set; }
    public Owner Owner { get; set; } = null!;

    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
