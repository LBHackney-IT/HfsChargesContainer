using HfsChargesContainer;
using HfsChargesContainer.Helpers;

try
{
    Console.WriteLine("Application started!\nConfiguring the Start up!");

    var entryPoint = new Startup()
        .ConfigureServices()
        .Build<ProcessEntryPoint>();

    Console.WriteLine("Ready to Run!");
    await entryPoint.Run();

    Console.WriteLine("Application finished!");
}
catch (Exception ex)
{
    LoggingHandler.LogError("An exception was caught at the program root level.");

    string environment = Environment.GetEnvironmentVariable("ENVIRONMENT")
        ?? throw new ArgumentNullException(nameof(environment));

    if (environment == "local")
        throw;

    LoggingHandler.LogError("Attempting to send out an SNS Nofication alert.");

    await EmailAlertsHandler.SendEmailAlert(ex, environment);

    // Throw to get the exception logged with the stack trace.
    throw;
}
