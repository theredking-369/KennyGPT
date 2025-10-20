using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MauiGPT.Services
{
    public class ThemeService : INotifyPropertyChanged
    {
        private bool _isDarkMode;
        private static ThemeService? _instance;

        public static ThemeService Instance => _instance ??= new ThemeService();

        public event PropertyChangedEventHandler? PropertyChanged;

        private ThemeService()
        {
            // Load saved theme preference
            _isDarkMode = Preferences.Get("dark_mode", false);
            ApplyTheme();
        }

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    Preferences.Set("dark_mode", value);
                    ApplyTheme();
                    OnPropertyChanged();
                }
            }
        }

        public void ToggleTheme()
        {
            IsDarkMode = !IsDarkMode;
        }

        private void ApplyTheme()
        {
            var app = Application.Current;
            if (app != null)
            {
                app.UserAppTheme = _isDarkMode ? AppTheme.Dark : AppTheme.Light;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
