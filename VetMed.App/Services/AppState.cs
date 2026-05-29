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

        try
        {
            SecureStorage.Default.Remove("auth_token");
            _ = Task.Run(() => SecureStorage.Default.SetAsync("auth_token", auth.Token));
        }
        catch { }

        OnChange?.Invoke();
    }

    public void ClearAuth()
    {
        Token = null;
        CurrentOwner = null;

        try
        {
            SecureStorage.Default.Remove("auth_token");
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
            // waliduj czy to prawdziwy JWT (3 segmenty oddzielone kropką)
            if (token.Split('.').Length != 3)
            {
                SecureStorage.Default.Remove("auth_token");
                return false;
            }
            Token = token;
            _api?.SetAuthToken(token);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
