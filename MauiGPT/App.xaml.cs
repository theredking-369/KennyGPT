using MauiGPT.Helpers;

namespace MauiGPT
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
            
            // Add global error handler for debugging
#if DEBUG
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                AppLogger.LogError("Unhandled Exception", e.ExceptionObject as Exception);
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                AppLogger.LogError("Unobserved Task Exception", e.Exception);
                e.SetObserved();
            };
#endif
        }
    }
}