using System.Globalization;

namespace MauiGPT.Converters
{
    public class MessageAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isUser)
            {
                return isUser ? LayoutOptions.End : LayoutOptions.Start;
            }
            return LayoutOptions.Start;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
