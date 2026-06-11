using VetMed.Shared.DTOs;
using VetMed.Shared.Enums;

namespace VetMed.App.Services;

public enum NotificationKind { Approved, Rejected }

public record NotificationItem(
    string Key,
    int VisitId,
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
    private HashSet<string>? _seen;

    public NotificationService(IVisitApiService visits) => _visits = visits;

    public List<NotificationItem> Items { get; private set; } = new();
    public int UnreadCount => Items.Count(i => !i.Read);

    public event Action? OnChange;

    public async Task RefreshAsync()
    {
        List<VisitDto> visits;
        try { visits = await _visits.GetVisitsAsync(); }
        catch { return; }

        var sources = visits
            .Where(v => v.Status is VisitStatus.Potwierdzona or VisitStatus.Odwolana)
            .Select(v => (
                Key: $"v{v.Id}:{(int)v.Status}",
                Visit: v,
                Kind: v.Status == VisitStatus.Potwierdzona ? NotificationKind.Approved : NotificationKind.Rejected))
            .ToList();

        _seen ??= LoadSeen();

        // Pierwsze uruchomienie: oznacz istniejące jako widziane, by nie zalać badge'a.
        if (!Preferences.Default.ContainsKey(SeenKey))
        {
            _seen = sources.Select(s => s.Key).ToHashSet();
            PersistSeen();
        }

        Items = sources
            .OrderByDescending(s => s.Visit.ScheduledAt)
            .Select(s => new NotificationItem(
                s.Key, s.Visit.Id, s.Kind, s.Visit.PetName, s.Visit.DoctorName,
                s.Visit.ScheduledAt, s.Visit.RejectionReason, _seen.Contains(s.Key)))
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
