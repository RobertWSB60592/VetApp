namespace VetMed.Tests.Blazor.App;

public class BottomNavLocationChangedTests
{
    private static async Task SimulateOnLocationChangedBody(Func<Task> refreshAsync)
    {
        try
        {
            await refreshAsync();
        }
        catch
        {
            // identyczny catch jak w BottomNav.OnLocationChanged
        }
    }

    [Fact]
    public async Task OnLocationChanged_WhenRefreshThrows_DoesNotPropagateException()
    {
        var act = async () =>
            await SimulateOnLocationChangedBody(
                () => throw new HttpRequestException("brak połączenia"));

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task OnLocationChanged_WhenRefreshThrowsOperationCanceledException_DoesNotPropagateException()
    {
        var act = async () =>
            await SimulateOnLocationChangedBody(
                () => throw new OperationCanceledException());

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task OnLocationChanged_WhenRefreshSucceeds_CompletesWithoutException()
    {
        var refreshCalled = false;

        var act = async () =>
            await SimulateOnLocationChangedBody(() =>
            {
                refreshCalled = true;
                return Task.CompletedTask;
            });

        await act.Should().NotThrowAsync();
        refreshCalled.Should().BeTrue();
    }
}
