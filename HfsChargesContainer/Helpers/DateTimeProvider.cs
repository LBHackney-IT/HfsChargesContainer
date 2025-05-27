
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("HfsChargesContainer.Tests")]
namespace HfsChargesContainer.Helpers
{
    public static class DateTimeProvider
    {
        private static Func<DateTime> _currentNow = () => DateTime.Now;
        private static Func<DateTime> _currentUtcNow = () => DateTime.UtcNow;
        private static Func<DateOnly> _currentToday = () => DateOnly.FromDateTime(DateTime.Now);

        public static DateTime Now => _currentNow();
        public static DateTime UtcNow => _currentUtcNow();
        public static DateOnly Today => _currentToday();

        internal static void SetSpecificDateTime(DateTime specificDateTime)
        {
            _currentNow = () => specificDateTime;
            _currentUtcNow = () => specificDateTime.ToUniversalTime();
            _currentToday = () => DateOnly.FromDateTime(specificDateTime);
        }

        internal static void ResetToSystemTime()
        {
            _currentNow = () => DateTime.Now;
            _currentUtcNow = () => DateTime.UtcNow;
            _currentToday = () => DateOnly.FromDateTime(DateTime.Now);
        }
    }
}