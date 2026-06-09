using Microsoft.JSInterop;

namespace VetMed.App.Services;

public class ThemeService
{
    private const string PrefKey = "theme_mode";
    private IJSRuntime? _js;
    private bool _initialized;

    public bool IsDark { get; private set; }

    public event Action? OnChange;

    public async Task InitializeAsync(IJSRuntime js)
    {
        _js = js;
        if (_initialized) { await ApplyAsync(); return; }

        try
        {
            IsDark = Preferences.Default.Get(PrefKey, "light") == "dark";
        }
        catch { IsDark = false; }

        _initialized = true;
        await ApplyAsync();
    }

    public async Task SetDarkAsync(bool dark)
    {
        IsDark = dark;
        try { Preferences.Default.Set(PrefKey, dark ? "dark" : "light"); } catch { }
        await ApplyAsync();
        OnChange?.Invoke();
    }

    public Task ToggleAsync() => SetDarkAsync(!IsDark);

    private async Task ApplyAsync()
    {
        if (_js is null) return;
        try { await _js.InvokeVoidAsync("vetmedTheme.set", IsDark ? "dark" : "light"); }
        catch { }
    }
}
