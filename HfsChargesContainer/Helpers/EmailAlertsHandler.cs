using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace HfsChargesContainer.Helpers
{
    public static class EmailAlertsHandler
    {
        private static readonly string _topicArn;
        private static readonly AmazonSimpleNotificationServiceClient _asnsClient;

        static EmailAlertsHandler()
        {
            _topicArn = Environment.GetEnvironmentVariable("SNS_TOPIC_ARN")
                ?? throw new ArgumentNullException(nameof(_topicArn));

            var clientConfig = new AmazonSimpleNotificationServiceConfig()
            {
                RegionEndpoint = RegionEndpoint.EUWest2,
            };

            _asnsClient = new AmazonSimpleNotificationServiceClient(clientConfig);
        }

        public static async Task SendEmailAlert(Exception ex, string environment)
        {
            var request = new PublishRequest
            {
                TopicArn = _topicArn,
                Message = ex.Message,
                Subject = $"[Warning!] Charges Ingest nightly process failure! [environment: {environment}]"
            };

            await _asnsClient.PublishAsync(request).ConfigureAwait(false);
        }
    }
}
