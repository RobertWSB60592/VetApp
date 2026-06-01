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
        // Android emulator: 10.0.2.2 → localhost hosta; fizyczny telefon: IP komputera
        const string ApiBase = "http://10.0.2.2:5100/";
#else
        // Produkcja: URL Cloud Run (uzupełnij po pierwszym deployu)
        const string ApiBase = "https://CLOUD_RUN_URL.run.app/";
#endif
        builder.Services.AddHttpClient<ApiClient>(c =>
            c.BaseAddress = new Uri(ApiBase));

        builder.Services.AddSingleton<AppState>();
        builder.Services.AddTransient<IAuthApiService, AuthApiService>();
        builder.Services.AddTransient<IPetApiService, PetApiService>();
        builder.Services.AddTransient<IVisitApiService, VisitApiService>();
        builder.Services.AddTransient<IDoctorApiService, DoctorApiService>();

        return builder.Build();
    }
}
