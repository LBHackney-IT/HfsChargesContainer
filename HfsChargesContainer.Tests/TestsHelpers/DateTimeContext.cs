using System;
using HfsChargesContainer.Helpers;

namespace HfsChargesContainer.Tests.TestsHelpers
{
    public sealed class DateTimeContext : IDisposable
    {
        public DateTimeContext(DateTime specificDateTimeToSet)
        {
            // Override the DateTimeProvider's time when this context is created.
            DateTimeProvider.SetSpecificDateTime(specificDateTimeToSet);
        }

        public void Dispose()
        {
            // Reset the DateTimeProvider to use the system's actual time when the context is disposed.
            DateTimeProvider.ResetToSystemTime();
        }
    }
}
