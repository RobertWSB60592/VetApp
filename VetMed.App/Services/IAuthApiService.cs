using VetMed.Shared.DTOs;

namespace VetMed.App.Services;

public interface IAuthApiService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);
}
