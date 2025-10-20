using System.Globalization;

namespace MauiGPT.Converters
{
    public class MessageTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isUser)
            {
                return isUser ? Colors.White : Color.FromArgb("#333333");
            }
            return Color.FromArgb("#333333");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
