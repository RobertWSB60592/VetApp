using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace VetMed.Clinic.Services;

public sealed class CookieAuthStateProvider : AuthenticationStateProvider
{
    private readonly Task<AuthenticationState> _state;

    public CookieAuthStateProvider(IHttpContextAccessor accessor)
    {
        var user = accessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
        _state = Task.FromResult(new AuthenticationState(user));
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() => _state;
}
