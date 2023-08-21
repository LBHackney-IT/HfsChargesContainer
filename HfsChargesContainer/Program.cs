using HfsChargesContainer;
using HfsChargesContainer.Helpers;

try
{
    LoggingHandler.LogError("Application started!\nConfiguring the Start up!");

    var entryPoint = new Startup()
        .ConfigureServices()
        .Build<ProcessEntryPoint>();

    LoggingHandler.LogError("Ready to Run!");
    await entryPoint.Run();

    LoggingHandler.LogError("Application finished!");
}
catch (Exception ex)
{
    LoggingHandler.LogError("An exception was caught at the program root level.");

    string? environment = Environment.GetEnvironmentVariable("ENVIRONMENT");

    if (environment is null)
        LoggingHandler.LogError($"Unset environment variable: '${nameof(environment)}'!");

    if (environment == "local")
        throw;

    LoggingHandler.LogError("Attempting to send out an SNS Nofication alert.");

    await EmailAlertsHandler.TrySendEmailAlert(ex, environment);

    LoggingHandler.LogError("\nApplication exception:\n");

    // Throw to get exception logged.
    throw;
}
