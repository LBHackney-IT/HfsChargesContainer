using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Google.Apis.Http;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using HfsChargesContainer.Gateways;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;

namespace HfsChargesContainer.Tests.Gateways
{
    public class GoogleClientServiceTests
    {
        [Fact]
        public async Task ReadSheetToEntitiesAsyncShouldTrimSpacesAndSkipMalformedRows()
        {
            // Arrange
            var spreadSheetId = "1234";
            var sheetName = "Sheet1";
            var range = "A1:B2";

            IList<IList<object>> values = new List<IList<object>>
            {
                new List<object> { "Prop1", "Prop2" },
                new List<object> { "  Value 1  ", "100" },
                new List<object> { "Bad Row", "NotANumber" },
                new List<object> { "Value 3", "200" }
            };

            var jsonResponse = JsonConvert.SerializeObject(new ValueRange { Values = values });

            var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(f => f.CreateHttpClient(It.IsAny<CreateHttpClientArgs>()))
                .Returns(new ConfigurableHttpClient(new ConfigurableMessageHandler(mockHandler.Object)));

            var initializer = new BaseClientService.Initializer
            {
                HttpClientFactory = mockHttpClientFactory.Object,
                ApplicationName = "Test"
            };

            var sheetsService = new SheetsService(initializer);
            var serviceUnderTest = new GoogleClientService(sheetsService);

            // Act
            var results = await serviceUnderTest
                .ReadSheetToEntitiesAsync<TestSheetEntity>(spreadSheetId, sheetName, range)
                .ConfigureAwait(false);

            // Assert
            results.Should().NotBeNull();
            results.Should().HaveCount(2);
            results[0].Prop1.Should().Be("Value 1");
            results[0].Prop2.Should().Be(100);
            results[1].Prop1.Should().Be("Value 3");
            results[1].Prop2.Should().Be(200);
        }

        public class TestSheetEntity
        {
            public string Prop1 { get; set; }
            public int Prop2 { get; set; }
        }
    }
}
