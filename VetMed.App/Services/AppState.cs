using VetMed.Shared.DTOs;

namespace VetMed.App.Services;

public class AppState
{
    public OwnerDto? CurrentOwner { get; private set; }
    public string? Token { get; private set; }
    public bool IsAuthenticated => Token is not null;

    public event Action? OnChange;

    public void SetAuth(AuthResponseDto auth)
    {
        Token = auth.Token;
        CurrentOwner = auth.Owner;

        try
        {
            SecureStorage.Default.Remove("auth_token");
            SecureStorage.Default.SetAsync("auth_token", auth.Token).GetAwaiter().GetResult();
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
            Token = token;
            return true;
        }
        catch
        {
            return false;
        }
    }
}
