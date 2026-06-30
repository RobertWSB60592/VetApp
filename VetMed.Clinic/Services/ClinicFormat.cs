using VetMed.Shared.Enums;

namespace VetMed.Clinic.Services;

public static class ClinicFormat
{
    public static string TypeLabel(VisitType t) => t switch
    {
        VisitType.Szczepienie => "Szczepienie",
        VisitType.Kontrola => "Kontrola",
        VisitType.Zabieg => "Zabieg",
        VisitType.Nagly => "Nagły",
        VisitType.Diagnostyka => "Diagnostyka",
        _ => t.ToString()
    };

    public static string StatusLabel(VisitStatus s) => s switch
    {
        VisitStatus.Oczekujaca => "Oczekująca",
        VisitStatus.Zaplanowana => "Zaplanowana",
        VisitStatus.Potwierdzona => "Potwierdzona",
        VisitStatus.Zakonczona => "Zakończona",
        VisitStatus.Odwolana => "Odwołana",
        _ => s.ToString()
    };

    public static string StatusBadgeClass(VisitStatus s) => s switch
    {
        VisitStatus.Oczekujaca => "bg-warning text-dark",
        VisitStatus.Zaplanowana => "bg-info text-dark",
        VisitStatus.Potwierdzona => "bg-success",
        VisitStatus.Zakonczona => "bg-secondary",
        VisitStatus.Odwolana => "bg-danger",
        _ => "bg-secondary"
    };

    public static string SpeciesEmoji(string? species)
    {
        var s = (species ?? string.Empty).ToLowerInvariant();
        if (s.Contains("pies") || s.Contains("psa") || s.Contains("dog")) return "🐶";
        if (s.Contains("kot")) return "🐱";
        if (s.Contains("król")) return "🐰";
        if (s.Contains("ptak")) return "🐦";
        return "🐾";
    }

    public static string DayLabel(DayOfWeek d) => d switch
    {
        DayOfWeek.Monday => "Poniedziałek",
        DayOfWeek.Tuesday => "Wtorek",
        DayOfWeek.Wednesday => "Środa",
        DayOfWeek.Thursday => "Czwartek",
        DayOfWeek.Friday => "Piątek",
        DayOfWeek.Saturday => "Sobota",
        DayOfWeek.Sunday => "Niedziela",
        _ => d.ToString()
    };
}
