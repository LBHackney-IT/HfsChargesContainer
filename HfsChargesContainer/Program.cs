using HfsChargesContainer;
using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

Console.WriteLine("Application started!\nConfiguring the Start up!");

// var entryPoint = new Startup()
//     .ConfigureServices()
//     .Build<ProcessEntryPoint>();

// Console.WriteLine("Ready to Run!");
// await entryPoint.Run();

// Console.Error.WriteLine("Test Error Log Behaviour");
Console.WriteLine(Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID")?.Substring(0,4));

string topicId = Environment.GetEnvironmentVariable("TOPIC_ARN");

Console.WriteLine(topicId?.Substring(0,7));

var clientConfig = new AmazonSimpleNotificationServiceConfig()
{
    RegionEndpoint = RegionEndpoint.EUWest2,
};


var asnsClient = new AmazonSimpleNotificationServiceClient(clientConfig);

string topicArn = topicId;
string messageText = @"Unhandled exception. Amazon.SimpleNotificationService.Model.InvalidParameterException: Invalid parameter: TopicArn
 ---> Amazon.Runtime.Internal.HttpErrorResponseException: Exception of type 'Amazon.Runtime.Internal.HttpErrorResponseException' was thrown.";

var request = new PublishRequest
{
    TopicArn = topicArn,
    Message = messageText,
    Subject = "Test subject"
};

await asnsClient.PublishAsync(request).ConfigureAwait(false);

Console.WriteLine("Application finished!");
