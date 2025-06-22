using HelloMauiApp.Services;
using HelloMauiApp.ViewModels;
using Microsoft.Extensions.Logging;
using SQLitePCL;

namespace HelloMauiApp;

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
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Bahnschrift.ttf", "AppFont");
            });

        raw.SetProvider(new SQLite3Provider_e_sqlite3());


#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<BmiRepository>();

        builder.Services.AddTransient<BmiCalculatorViewModel>();
        builder.Services.AddTransient<BmiCalculatorPage>();

        builder.Services.AddTransient<HistoryViewModel>();
        builder.Services.AddTransient<HistoryPage>();

        return builder.Build();
    }
}