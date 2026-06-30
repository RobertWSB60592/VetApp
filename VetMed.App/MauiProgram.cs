using Microsoft.Extensions.Logging;
using VetMed.App.Services;

namespace VetMed.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

#if DEBUG
        // Android emulator: 10.0.2.2 → localhost hosta; fizyczny telefon: IP komputera w sieci
        const string ApiBase = "http://10.0.2.2:5100/";
#else
        const string ApiBase = "https://vetmed-api-x4fdlckhfq-lm.a.run.app/";
#endif

        builder.Services.AddSingleton<ApiClient>(_ =>
            new ApiClient(new HttpClient { BaseAddress = new Uri(ApiBase) }));

        builder.Services.AddSingleton<AppState>();
        builder.Services.AddSingleton<ThemeService>();
        builder.Services.AddSingleton<NotificationService>();
        builder.Services.AddTransient<IAuthApiService, AuthApiService>();
        builder.Services.AddTransient<IPetApiService, PetApiService>();
        builder.Services.AddTransient<IVisitApiService, VisitApiService>();
        builder.Services.AddTransient<IDoctorApiService, DoctorApiService>();
        builder.Services.AddTransient<IHealthApiService, HealthApiService>();

        var app = builder.Build();
        var appState = app.Services.GetRequiredService<AppState>();
        var apiClient = app.Services.GetRequiredService<ApiClient>();
        appState.SetApiClient(apiClient);

        return app;
    }
}
