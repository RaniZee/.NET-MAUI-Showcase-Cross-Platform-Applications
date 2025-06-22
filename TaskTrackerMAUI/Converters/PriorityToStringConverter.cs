using System;
using System.Globalization;
using TaskTrackerMAUI.Models;

namespace TaskTrackerMAUI.Converters
{
    public class PriorityToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Priority priority)
            {
                return priority switch
                {
                    Priority.Low => "Низкий",
                    Priority.Medium => "Средний",
                    Priority.High => "Высокий",
                    Priority.Critical => "Критический",
                    _ => string.Empty
                };
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}