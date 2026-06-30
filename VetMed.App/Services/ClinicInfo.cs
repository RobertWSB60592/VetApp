namespace VetMed.App.Services;

/// <summary>Dane przychodni, do której należy klient. Jedna klinika dla całej aplikacji.</summary>
public static class ClinicInfo
{
    public const string Name = "VETMED";
    public const string City = "Bydgoszcz";
    public const string FullName = "VETMED Bydgoszcz";
    public const string Address = "ul. Zwierzyniecka 12, Bydgoszcz";

    public const string HoursWeekday = "8:00 – 20:00";
    public const string HoursSaturday = "9:00 – 14:00";
    public const string HoursSunday = "Nieczynne";

    public const string Phone = "+48 52 345 67 89";
    public const string PhoneDial = "+48523456789";
    public const string Email = "kontakt@vetmed.pl";
    public const string MapsUrl = "https://www.google.com/maps/search/Zwierzyniecka+12+Bydgoszcz";
}
