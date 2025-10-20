using System.Globalization;

namespace MauiGPT.Converters
{
    public class ThemeIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isDarkMode)
            {
                return isDarkMode ? "??" : "??";
            }
            return "??";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
