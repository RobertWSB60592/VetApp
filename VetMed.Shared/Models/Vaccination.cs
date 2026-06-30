namespace VetMed.Shared.Models;

/// <summary>Szczepienie wykonane u zwierzęcia, z opcjonalnym terminem kolejnej dawki.</summary>
public class Vaccination
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly AdministeredOn { get; set; }
    public DateOnly? NextDueOn { get; set; }
    public string? Notes { get; set; }

    public int PetId { get; set; }
    public Pet Pet { get; set; } = null!;
}
