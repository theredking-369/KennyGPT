using System.Diagnostics;

namespace MauiGPT.Helpers
{
    public static class AppLogger
    {
        public static void Log(string message, string category = "INFO")
        {
#if DEBUG
            Debug.WriteLine($"[{category}] {DateTime.Now:HH:mm:ss} - {message}");
#endif
        }

        public static void LogError(string message, Exception? ex = null)
        {
#if DEBUG
            Debug.WriteLine($"[ERROR] {DateTime.Now:HH:mm:ss} - {message}");
            if (ex != null)
            {
                Debug.WriteLine($"Exception: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            }
#endif
        }
    }
}
