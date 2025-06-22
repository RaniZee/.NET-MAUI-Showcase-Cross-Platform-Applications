using System;
using System.Globalization;
using TaskTrackerMAUI.Models;
using TaskStatus = TaskTrackerMAUI.Models.TaskStatus;

namespace TaskTrackerMAUI.Converters
{
    public class StatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TaskStatus status)
            {
                return status switch
                {
                    TaskStatus.New => "Новая",
                    TaskStatus.InProgress => "В работе",
                    TaskStatus.OnReview => "На проверке",
                    TaskStatus.Completed => "Завершена",
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