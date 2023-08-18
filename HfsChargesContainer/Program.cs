using HfsChargesContainer;
using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using System;

Console.WriteLine("Application started!\nConfiguring the Start up!");

try
{
    string message = @"Testing length and whitespace: Lass Blimey jib square-rigged loot galleon aye long clothes capstan Shiver me timbers. Code of conduct measured fer yer chains pillage overhaul ballast nipper Buccaneer driver Sink me case shot. Barbary Coast sloop Jack Ketch jolly boat loaded to the gunwalls knave heave down mizzenmast mizzen draught.

Grog bilge water knave six pounders prow Spanish Main ye jib mizzenmast Admiral of the Black. Corsair tackle hail-shot salmagundi list spyglass jack lugger topmast long boat. Keel bring a spring upon her cable topsail scourge of the seven seas mizzen Spanish Main lass Jolly Roger jack mutiny.

Measured fer yer chains pressgang bilged on her anchor gibbet reef dance the hempen jig brigantine league weigh anchor aye. Pillage clipper crow's nest Blimey lee shrouds gun sheet code of conduct spanker. Chantey crimp draft Cat o'nine tails spirits fathom Jack Ketch hulk gangway parley.";
        
    throw new PirateLoremIpsumException(message);
}
catch (Exception ex)
{
    string topicId = Environment.GetEnvironmentVariable("SNS_TOPIC_ARN");

    Console.WriteLine(topicId?.Substring(0,7));

    var clientConfig = new AmazonSimpleNotificationServiceConfig()
    {
        RegionEndpoint = RegionEndpoint.EUWest2,
    };


    var asnsClient = new AmazonSimpleNotificationServiceClient(clientConfig);

    string topicArn = topicId;

    var request = new PublishRequest
    {
        TopicArn = topicArn,
        Message = ex.Message,
        Subject = "[Warning!] Charges Ingest nightly process failure! [housing-development]"
    };

    await asnsClient.PublishAsync(request).ConfigureAwait(false);
}

Console.WriteLine("Application finished!");

public class PirateLoremIpsumException : Exception
{
    public PirateLoremIpsumException(string message) : base(message) {}
}