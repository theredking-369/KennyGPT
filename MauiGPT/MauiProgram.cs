using Microsoft.Extensions.Logging;
using MauiGPT.Interfaces;
using MauiGPT.Services;
using MauiGPT.ViewModels;
using MauiGPT.Views;

namespace MauiGPT
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
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Register Services
            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddSingleton<IAzureService, AzureService>();
            builder.Services.AddSingleton<ThemeService>(sp => ThemeService.Instance);

            // Register ViewModels
            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddTransient<ChatViewModel>();

            // Register Views
            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddTransient<ChatPage>();

            return builder.Build();
        }
    }
}
