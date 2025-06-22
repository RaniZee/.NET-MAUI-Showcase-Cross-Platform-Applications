using TaskTrackerMAUI.Models;

namespace TaskTrackerMAUI.Extensions
{
    public static class EnumsExtensions
    {
        public static string ToRussianString(this Priority priority)
        {
            return priority switch
            {
                Priority.Low => "Низкий",
                Priority.Medium => "Средний",
                Priority.High => "Высокий",
                Priority.Critical => "Критический",
                _ => priority.ToString()
            };
        }
    }
}