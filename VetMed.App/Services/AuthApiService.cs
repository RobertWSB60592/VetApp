using VetMed.Shared.DTOs;

namespace VetMed.App.Services;

public class AuthApiService : IAuthApiService
{
    private readonly ApiClient _api;

    public AuthApiService(ApiClient api)
    {
        _api = api;
    }

    public Task<AuthResponseDto?> LoginAsync(LoginDto dto) =>
        _api.PostAsync<AuthResponseDto>("api/auth/login", dto);

    public Task<AuthResponseDto?> RegisterAsync(RegisterDto dto) =>
        _api.PostAsync<AuthResponseDto>("api/auth/register", dto);
}
