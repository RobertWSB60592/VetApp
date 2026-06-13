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

public class AllVisitsTests : TestContext
{
    private readonly Mock<IClinicService> _clinic = new();

    public AllVisitsTests()
    {
        Services.AddSingleton(_clinic.Object);

        Services.AddSingleton<IAuthorizationPolicyProvider, DefaultAuthorizationPolicyProvider>();
        Services.AddSingleton<IAuthorizationService, DefaultAuthorizationService>();
        Services.AddSingleton<AuthenticationStateProvider>(
            new FakeAuthenticationStateProvider(isAuthenticated: true));
        Services.AddAuthorizationCore();
        Services.AddCascadingAuthenticationState();
    }

    private static ClinicVisitVm MakeVisit(int id, VisitStatus status = VisitStatus.Potwierdzona) => new(
        id,
        DateTime.Now.AddDays(1),
        VisitType.Kontrola,
        status,
        null, null, null,
        "Ryszard", "Kot", "Anna Nowak", null, "dr Kowalska");

    private void SetupDefaults(List<ClinicVisitVm>? visits = null)
    {
        _clinic.Setup(s => s.GetDoctorOptionsAsync(default))
            .ReturnsAsync(new List<(int Id, string Name)>());

        _clinic.Setup(s => s.GetAllAsync(null, null, default))
            .ReturnsAsync(visits ?? new List<ClinicVisitVm>());
    }

    [Fact]
    public async Task SaveCompleteAsync_WhenCompleteThrows_SavingResetToFalse()
    {
        var visit = MakeVisit(10, VisitStatus.Potwierdzona);
        SetupDefaults(new List<ClinicVisitVm> { visit });

        _clinic.Setup(s => s.CompleteAsync(10, It.IsAny<string?>(), default))
            .ThrowsAsync(new TimeoutException("timeout bazy"));

        var cut = RenderComponent<AllVisits>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        await cut.InvokeAsync(() => cut.Find("button.btn-outline-success").Click());

        try
        {
            await cut.InvokeAsync(() => cut.Find("button.btn-success").Click());
        }
        catch (TimeoutException) { }

        cut.Find("button.btn-success")
            .HasAttribute("disabled")
            .Should().BeFalse("saving musi wrócić do false mimo wyjątku z CompleteAsync");
    }

    [Fact]
    public async Task SaveCompleteAsync_WhenUpdateSummaryThrows_SavingResetToFalse()
    {
        var visit = MakeVisit(11, VisitStatus.Zakonczona);
        SetupDefaults(new List<ClinicVisitVm> { visit });

        _clinic.Setup(s => s.UpdateSummaryAsync(11, It.IsAny<string?>(), default))
            .ThrowsAsync(new InvalidOperationException("błąd zapisu"));

        var cut = RenderComponent<AllVisits>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        await cut.InvokeAsync(() => cut.Find("button.btn-outline-secondary").Click());

        try
        {
            await cut.InvokeAsync(() => cut.Find("button.btn-success").Click());
        }
        catch (InvalidOperationException) { }

        cut.Find("button.btn-success")
            .HasAttribute("disabled")
            .Should().BeFalse("saving musi wrócić do false mimo wyjątku z UpdateSummaryAsync");
    }

    [Fact]
    public async Task SaveCompleteAsync_WhenCompleteSucceeds_FormCollapses()
    {
        var visit = MakeVisit(12, VisitStatus.Potwierdzona);

        var callCount = 0;
        _clinic.Setup(s => s.GetDoctorOptionsAsync(default))
            .ReturnsAsync(new List<(int Id, string Name)>());
        _clinic.Setup(s => s.GetAllAsync(null, null, default))
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount == 1
                    ? new List<ClinicVisitVm> { visit }
                    : new List<ClinicVisitVm> { MakeVisit(12, VisitStatus.Zakonczona) };
            });

        _clinic.Setup(s => s.CompleteAsync(12, It.IsAny<string?>(), default))
            .ReturnsAsync(true);

        var cut = RenderComponent<AllVisits>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        await cut.InvokeAsync(() => cut.Find("button.btn-outline-success").Click());
        await cut.InvokeAsync(() => cut.Find("button.btn-success").Click());

        cut.FindAll("textarea").Should().BeEmpty("formularz powinien zniknąć po udanym zapisie");
    }
}
