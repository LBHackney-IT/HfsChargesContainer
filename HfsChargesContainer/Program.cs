using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
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

    string environment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? throw new ArgumentNullException(nameof(environment));

    if (environment == "local")
        throw;

    LoggingHandler.LogError("Attempting to send out an SNS Nofication alert.");

    var clientConfig = new AmazonSimpleNotificationServiceConfig()
    {
        RegionEndpoint = RegionEndpoint.EUWest2,
    };

    var asnsClient = new AmazonSimpleNotificationServiceClient(clientConfig);

    string topicArn = Environment.GetEnvironmentVariable("SNS_TOPIC_ARN") ?? throw new ArgumentNullException(nameof(topicArn));

    var request = new PublishRequest
    {
        TopicArn = topicArn,
        Message = ex.Message,
        Subject = $"[Warning!] Charges Ingest nightly process failure! [environment: {environment}]"
    };

    await asnsClient.PublishAsync(request).ConfigureAwait(false);

    // Throw again to get the exception logged with the stack trace.
    throw;
}
