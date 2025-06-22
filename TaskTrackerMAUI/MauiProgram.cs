using Microsoft.Extensions.Logging;
using TaskTrackerMAUI.ViewModels;
using TaskTrackerMAUI.Views;
namespace TaskTrackerMAUI
{
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
                    fonts.AddFont("bahnschrift.ttf", "Bahnschrift");
                });

            builder.Services.AddSingleton<KanbanViewModel>();       

            builder.Services.AddSingleton<KanbanPage>();        

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}