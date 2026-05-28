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

        builder.Services.AddHttpClient<ApiClient>(c =>
            c.BaseAddress = new Uri("http://localhost:5100/"));

        builder.Services.AddSingleton<AppState>();
        builder.Services.AddTransient<IAuthApiService, AuthApiService>();
        builder.Services.AddTransient<IPetApiService, PetApiService>();
        builder.Services.AddTransient<IVisitApiService, VisitApiService>();
        builder.Services.AddTransient<IDoctorApiService, DoctorApiService>();

        return builder.Build();
    }
}
