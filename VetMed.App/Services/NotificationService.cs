using VetMed.Shared.DTOs;
using VetMed.Shared.Enums;

namespace VetMed.App.Services;

public enum NotificationKind { Approved, Rejected, VaccineDue, Completed }

public record NotificationItem(
    string Key,
    int VisitId,
    int PetId,
    NotificationKind Kind,
    string PetName,
    string DoctorName,
    DateTime ScheduledAt,
    string? Reason,
    bool Read);

public class NotificationService
{
    private const string SeenKey = "seen_notifications";

    private readonly IVisitApiService _visits;
    private readonly IHealthApiService _health;
    private HashSet<string>? _seen;

    public NotificationService(IVisitApiService visits, IHealthApiService health)
    {
        _visits = visits;
        _health = health;
    }

    public List<NotificationItem> Items { get; private set; } = new();
    public int UnreadCount => Items.Count(i => !i.Read);

    public event Action? OnChange;

    public async Task RefreshAsync()
    {
        List<VisitDto> visits;
        try { visits = await _visits.GetVisitsAsync(); }
        catch { return; }

        List<VaccinationDto> upcoming;
        try { upcoming = await _health.GetUpcomingVaccinationsAsync(30); }
        catch { upcoming = new(); }

        var sources = visits
            .Where(v => v.Status is VisitStatus.Potwierdzona or VisitStatus.Odwolana
                || (v.Status == VisitStatus.Zakonczona && !string.IsNullOrWhiteSpace(v.DoctorSummary)))
            .Select(v => new NotificationItem(
                $"v{v.Id}:{(int)v.Status}",
                v.Id, v.PetId,
                v.Status switch
                {
                    VisitStatus.Potwierdzona => NotificationKind.Approved,
                    VisitStatus.Zakonczona   => NotificationKind.Completed,
                    _                        => NotificationKind.Rejected
                },
                v.PetName, v.DoctorName, v.ScheduledAt,
                v.Status == VisitStatus.Zakonczona ? v.DoctorSummary : v.RejectionReason,
                false))
            .Concat(upcoming
                .Where(u => u.NextDueOn is not null)
                .Select(u => new NotificationItem(
                    $"vac{u.Id}:{u.NextDueOn:yyyy-MM-dd}",
                    0, u.PetId,
                    NotificationKind.VaccineDue,
                    u.PetName, u.Name,
                    u.NextDueOn!.Value.ToDateTime(TimeOnly.MinValue),
                    null, false)))
            .ToList();

        _seen ??= LoadSeen();

        // Pierwsze uruchomienie: oznacz istniejące jako widziane, by nie zalać badge'a.
        if (!Preferences.Default.ContainsKey(SeenKey))
        {
            _seen = sources.Select(s => s.Key).ToHashSet();
            PersistSeen();
        }

        Items = sources
            .OrderByDescending(s => s.ScheduledAt)
            .Select(s => s with { Read = _seen.Contains(s.Key) })
            .ToList();

        OnChange?.Invoke();
    }

    public void MarkRead(string key)
    {
        _seen ??= LoadSeen();
        if (!_seen.Add(key)) return;
        PersistSeen();
        Items = Items.Select(i => i.Key == key ? i with { Read = true } : i).ToList();
        OnChange?.Invoke();
    }

    public void MarkAllRead()
    {
        _seen ??= LoadSeen();
        var changed = false;
        foreach (var i in Items) changed |= _seen.Add(i.Key);
        if (!changed) return;
        PersistSeen();
        Items = Items.Select(i => i with { Read = true }).ToList();
        OnChange?.Invoke();
    }

    public void Reset()
    {
        _seen = null;
        Items = new();
        OnChange?.Invoke();
    }

    private static HashSet<string> LoadSeen()
    {
        var raw = Preferences.Default.Get(SeenKey, string.Empty);
        return raw.Split(',', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
    }

    private void PersistSeen() => Preferences.Default.Set(SeenKey, string.Join(',', _seen!));
}
