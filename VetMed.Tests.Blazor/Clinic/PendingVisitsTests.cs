using System.Security.Claims;
using Bunit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VetMed.Clinic.Components.Pages;
using VetMed.Clinic.Services;
using VetMed.Shared.Enums;

namespace VetMed.Tests.Blazor.Clinic;

public class PendingVisitsTests : TestContext
{
    private readonly Mock<IClinicService> _clinic = new();

    public PendingVisitsTests()
    {
        Services.AddSingleton(_clinic.Object);

        Services.AddSingleton<IAuthorizationPolicyProvider, DefaultAuthorizationPolicyProvider>();
        Services.AddSingleton<IAuthorizationService, DefaultAuthorizationService>();
        Services.AddSingleton<AuthenticationStateProvider>(
            new FakeAuthenticationStateProvider(isAuthenticated: true));
        Services.AddAuthorizationCore();
        Services.AddCascadingAuthenticationState();
    }

    private static ClinicVisitVm MakeVisit(int id) => new(
        id,
        DateTime.Now.AddDays(1),
        VisitType.Kontrola,
        VisitStatus.Oczekujaca,
        null, null, null,
        "Ozzy", "Pies", "Jan Kowalski", null, "dr Nowak");

    [Fact]
    public async Task Approve_WhenServiceThrows_BusyResetToFalse()
    {
        _clinic.Setup(s => s.GetPendingAsync(default))
            .ReturnsAsync(new List<ClinicVisitVm> { MakeVisit(1) });

        _clinic.Setup(s => s.ApproveAsync(1, default))
            .ThrowsAsync(new HttpRequestException("brak połączenia"));

        var cut = RenderComponent<PendingVisits>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        var approveButton = cut.Find("button.btn-success");
        approveButton.HasAttribute("disabled").Should().BeFalse();

        try
        {
            await cut.InvokeAsync(() => approveButton.Click());
        }
        catch (HttpRequestException) { }

        cut.Find("button.btn-success")
            .HasAttribute("disabled")
            .Should().BeFalse("busy musi wrócić do false mimo wyjątku z ApproveAsync");
    }

    [Fact]
    public async Task ConfirmReject_WhenServiceThrows_BusyResetToFalse()
    {
        _clinic.Setup(s => s.GetPendingAsync(default))
            .ReturnsAsync(new List<ClinicVisitVm> { MakeVisit(2) });

        _clinic.Setup(s => s.RejectAsync(2, It.IsAny<string?>(), default))
            .ThrowsAsync(new InvalidOperationException("błąd bazy"));

        var cut = RenderComponent<PendingVisits>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        await cut.InvokeAsync(() => cut.Find("button.btn-outline-danger").Click());

        try
        {
            await cut.InvokeAsync(() => cut.Find("button.btn-danger").Click());
        }
        catch (InvalidOperationException) { }

        cut.Find("button.btn-danger")
            .HasAttribute("disabled")
            .Should().BeFalse("busy musi wrócić do false mimo wyjątku z RejectAsync");
    }

    [Fact]
    public async Task Approve_WhenServiceSucceeds_ListReloads()
    {
        var callCount = 0;
        _clinic.Setup(s => s.GetPendingAsync(default))
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount == 1
                    ? new List<ClinicVisitVm> { MakeVisit(3) }
                    : new List<ClinicVisitVm>();
            });

        _clinic.Setup(s => s.ApproveAsync(3, default)).ReturnsAsync(true);

        var cut = RenderComponent<PendingVisits>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        await cut.InvokeAsync(() => cut.Find("button.btn-success").Click());

        cut.FindAll(".card").Should().BeEmpty("lista powinna być pusta po zatwierdzeniu");
    }
}

internal sealed class FakeAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly bool _isAuthenticated;

    public FakeAuthenticationStateProvider(bool isAuthenticated) =>
        _isAuthenticated = isAuthenticated;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = _isAuthenticated
            ? new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, "Staff"), new Claim(ClaimTypes.Role, "Admin") },
                "FakeScheme")
            : new ClaimsIdentity();

        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }
}
