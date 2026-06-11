using Microsoft.Extensions.DependencyInjection;
using VetMed.App.Services;

namespace VetMed.App;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new MainPage()) { Title = "VetMed.App" };

		window.Resumed += async (_, _) =>
		{
			try
			{
				var notif = IPlatformApplication.Current?.Services.GetService<NotificationService>();
				if (notif is not null)
					await notif.RefreshAsync();
			}
			catch { }
		};

		return window;
	}
}
