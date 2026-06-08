using VetMed.Shared.Enums;

namespace VetMed.Shared.Models;

/// <summary>Wizyta weterynaryjna powiązana ze zwierzęciem i lekarzem.</summary>
public class Visit
{
    public int Id { get; set; }
    public DateTime ScheduledAt { get; set; }
    public VisitType Type { get; set; }
    public VisitStatus Status { get; set; } = VisitStatus.Zaplanowana;
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }

    public int PetId { get; set; }
    public Pet Pet { get; set; } = null!;

    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
}
