namespace VetMed.Shared.Models;

/// <summary>Recepta / lek przepisany zwierzęciu przez klinikę.</summary>
public class Prescription
{
    public int Id { get; set; }
    public string Medication { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public DateOnly StartsOn { get; set; }
    public DateOnly? EndsOn { get; set; }
    public string? Notes { get; set; }

    public int PetId { get; set; }
    public Pet Pet { get; set; } = null!;
}
