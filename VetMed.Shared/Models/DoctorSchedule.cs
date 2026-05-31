namespace VetMed.Shared.Models;

/// <summary>Tygodniowy szablon dostępności lekarza dla pojedynczego dnia tygodnia.</summary>
public class DoctorSchedule
{
    public int Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
}
