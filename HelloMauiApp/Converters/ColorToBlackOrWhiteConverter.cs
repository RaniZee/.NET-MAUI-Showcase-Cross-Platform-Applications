using System.Globalization;

namespace HelloMauiApp.Converters;

public class ColorToBlackOrWhiteConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Color color)
        {

            return (color.Red * 0.299 + color.Green * 0.587 + color.Blue * 0.114) > 0.6 ? Colors.Black : Colors.White;
        }
        return Colors.Black;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}