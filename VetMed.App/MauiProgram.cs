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

#if ANDROID
        var apiBase = "http://192.168.1.11:5100/";
#else
        var apiBase = "http://127.0.0.1:5100/";
#endif

        builder.Services.AddSingleton<ApiClient>(_ =>
            new ApiClient(new HttpClient { BaseAddress = new Uri(apiBase) }));

        builder.Services.AddSingleton<AppState>();
        builder.Services.AddTransient<IAuthApiService, AuthApiService>();
        builder.Services.AddTransient<IPetApiService, PetApiService>();
        builder.Services.AddTransient<IVisitApiService, VisitApiService>();
        builder.Services.AddTransient<IDoctorApiService, DoctorApiService>();

        var app = builder.Build();
        var appState = app.Services.GetRequiredService<AppState>();
        var apiClient = app.Services.GetRequiredService<ApiClient>();
        appState.SetApiClient(apiClient);

        return app;
    }
}
