using System;

namespace HfsChargesContainer.Helpers
{
    public static class LoggingHandler
    {
        public static void LogError(string message)
        {
            Console.Error.WriteLine($"[ERROR]: {message}");
        }

        public static void LogWarning(string message)
        {
            Console.WriteLine($"[WARNING]: {message}");
        }

        public static void LogInfo(string message)
        {
            Console.WriteLine($"[INFO]: {message}");
        }
    }
}
