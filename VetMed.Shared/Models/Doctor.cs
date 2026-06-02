namespace VetMed.Shared.Models;

/// <summary>Lekarz weterynarii przyjmujący w klinice.</summary>
public class Doctor
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;

    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
    public ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();
}
