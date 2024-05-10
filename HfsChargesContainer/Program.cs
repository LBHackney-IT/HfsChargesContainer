using HfsChargesContainer;
using HfsChargesContainer.Helpers;

try
{
    LoggingHandler.LogInfo("Application started!\nConfiguring the Start up!");

    var entryPoint = new Startup()
        .ConfigureServices()
        .Build<ProcessEntryPoint>();

    LoggingHandler.LogInfo("Ready to Run!");
    await entryPoint.Run();

    LoggingHandler.LogInfo("Application finished!");
}
catch (Exception ex)
{
    LoggingHandler.LogError("An exception was caught at the program root level.");

    string? environment = Environment.GetEnvironmentVariable("ENVIRONMENT");

    if (environment is null)
        LoggingHandler.LogError($"Unset environment variable: '${nameof(environment)}'!");

    if (environment == "local")
        throw;

    LoggingHandler.LogInfo("Attempting to send out an SNS Nofication alert.");

    await EmailAlertsHandler.TrySendEmailAlert(ex, environment);

    LoggingHandler.LogError("\nApplication exception:\n");

    LoggingHandler.LogError(
        $"Exception Type: {ex.GetType().Name}\nMessage: {ex.Message}\nStack Trace: {ex.StackTrace}\n\n");
    
    // Throw to terminate with non-zero exit
    throw;
}
