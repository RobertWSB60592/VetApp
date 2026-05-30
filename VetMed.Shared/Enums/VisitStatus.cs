namespace VetMed.Shared.Enums;

/// <summary>Status wizyty w cyklu życia rezerwacji.</summary>
public enum VisitStatus
{
    Zaplanowana = 0,
    Potwierdzona = 1,
    Zakonczona = 2,
    Odwolana = 3,
    Oczekujaca = 4
}
