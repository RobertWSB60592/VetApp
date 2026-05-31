using System.Text.Json;
using VetMed.Shared.DTOs;

namespace VetMed.App.Services;

public class AppState
{
    private ApiClient? _api;

    public void SetApiClient(ApiClient api) => _api = api;

    public OwnerDto? CurrentOwner { get; private set; }
    public string? Token { get; private set; }
    public bool IsAuthenticated => Token is not null;

    public event Action? OnChange;

    public void SetAuth(AuthResponseDto auth)
    {
        Token = auth.Token;
        CurrentOwner = auth.Owner;
        _api?.SetAuthToken(auth.Token);

        _ = Task.Run(async () =>
        {
            try
            {
                SecureStorage.Default.Remove("auth_token");
                await SecureStorage.Default.SetAsync("auth_token", auth.Token);
                Preferences.Default.Set("owner_json", JsonSerializer.Serialize(auth.Owner));
            }
            catch { }
        });

        OnChange?.Invoke();
    }

    public void ClearAuth()
    {
        Token = null;
        CurrentOwner = null;

        try
        {
            SecureStorage.Default.Remove("auth_token");
            Preferences.Default.Remove("owner_json");
        }
        catch { }

        OnChange?.Invoke();
    }

    public async Task<bool> TryRestoreAsync()
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync("auth_token");
            if (token is null) return false;

            if (token.Split('.').Length != 3)
            {
                SecureStorage.Default.Remove("auth_token");
                Preferences.Default.Remove("owner_json");
                return false;
            }

            // sprawdź czy token nie wygasł
            var expiry = GetTokenExpiry(token);
            if (expiry is not null && expiry < DateTime.UtcNow)
            {
                SecureStorage.Default.Remove("auth_token");
                Preferences.Default.Remove("owner_json");
                return false;
            }

            Token = token;
            _api?.SetAuthToken(token);

            var ownerJson = Preferences.Default.Get("owner_json", string.Empty);
            if (!string.IsNullOrEmpty(ownerJson))
                CurrentOwner = JsonSerializer.Deserialize<OwnerDto>(ownerJson);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static DateTime? GetTokenExpiry(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3) return null;
            var payload = parts[1];
            var padded = payload.PadRight((payload.Length + 3) / 4 * 4, '=');
            var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(padded));
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("exp", out var expEl)) return null;
            return DateTimeOffset.FromUnixTimeSeconds(expEl.GetInt64()).UtcDateTime;
        }
        catch { return null; }
    }
}
