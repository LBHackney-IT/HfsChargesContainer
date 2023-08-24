using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace HfsChargesContainer.Helpers
{
    public static class EmailAlertsHandler
    {
        private static readonly string? _topicArn;
        private static readonly AmazonSimpleNotificationServiceClient _asnsClient;

        static EmailAlertsHandler()
        {
            _topicArn = Environment.GetEnvironmentVariable("SNS_TOPIC_ARN");

            var clientConfig = new AmazonSimpleNotificationServiceConfig()
            {
                RegionEndpoint = RegionEndpoint.EUWest2,
            };

            _asnsClient = new AmazonSimpleNotificationServiceClient(clientConfig);
        }

        /// <summary>
        /// Surrounding the 'SendEmailAlert' with a try-catch to avoid losing the
        /// Initial exception 'initialEx' that triggered the Email Sending to begin with.
        /// </summary>
        public static async Task TrySendEmailAlert(Exception initialEx, string? environment)
        {
            try
            {
                await SendEmailAlert(initialEx, environment);
            }
            catch (Exception secondaryEx)
            {
                string secondaryMessage =
                    "Failed to send an Email Alert due an inner SNS exception:\n\n" +
                    secondaryEx.ToString();

                LoggingHandler.LogError(secondaryMessage);
            }
        }

        public static async Task SendEmailAlert(Exception ex, string? environment = "unset")
        {
            if (_topicArn is null)
                throw new ArgumentNullException(nameof(_topicArn));

            var request = new PublishRequest
            {
                TopicArn = _topicArn,
                Message = ex.Message,
                Subject = $"[Error!] Charges Ingest nightly process failure! [environment: {environment}]"
            };

            await _asnsClient.PublishAsync(request).ConfigureAwait(false);
        }
    }
}
