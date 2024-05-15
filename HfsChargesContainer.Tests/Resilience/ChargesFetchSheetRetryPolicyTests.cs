using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HfsChargesContainer.Domain;
using HfsChargesContainer.Gateways.Interfaces;
using HfsChargesContainer.UseCases;
using HfsChargesContainer.UseCases.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Polly;
using Xunit;

namespace HfsChargesContainer.Tests.Resilience
{
    public class ChargesFetchSheetRetryPolicyTests
    {
        Mock<IBatchLogGateway> _mockBatchLogGateway;
        Mock<IBatchLogErrorGateway> _mockBatchLogErrorGateway;
        Mock<IChargesBatchYearsGateway> _mockChargesBatchYearsGateway;
        Mock<IChargesGateway> _mockChargesGateway;
        Mock<IGoogleFileSettingGateway> _mockGoogleFileSettingGateway;
        Mock<IGoogleClientService> _mockGoogleClientService;
        ILoadChargesUseCase _ucWithTestedRetryPolicy;


        public ChargesFetchSheetRetryPolicyTests()
        {            
            _mockBatchLogGateway = new Mock<IBatchLogGateway>();
            _mockBatchLogErrorGateway = new Mock<IBatchLogErrorGateway>();
            _mockChargesBatchYearsGateway = new Mock<IChargesBatchYearsGateway>();
            _mockChargesGateway = new Mock<IChargesGateway>();
            _mockGoogleFileSettingGateway = new Mock<IGoogleFileSettingGateway>();
            _mockGoogleClientService = new Mock<IGoogleClientService>();

            Environment.SetEnvironmentVariable("ENVIRONMENT", "tests");

            var services = new ServiceCollection();
            var serviceProvider = new Startup(services);

            serviceProvider.ConfigureResiliencePolicies(services);
            var builtProvider = services.BuildServiceProvider();

            var fetchSheetRetryPolicy = builtProvider.GetService<IAsyncPolicy<IList<ChargesAuxDomain>>>();

            _ucWithTestedRetryPolicy = new LoadChargesUseCase(
                _mockBatchLogGateway.Object,
                _mockBatchLogErrorGateway.Object,
                _mockChargesBatchYearsGateway.Object,
                _mockChargesGateway.Object,
                _mockGoogleFileSettingGateway.Object,
                _mockGoogleClientService.Object,
                fetchSheetRetryPolicy
            );
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task FetchChargesSheetRetryPolicyRetriesTheFetchUponGoogleClientServiceReadSheetsFailure(int failureCount)
        {
            // arrange
            _mockBatchLogGateway
                .Setup(bl => bl.CreateAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(RandomGen.Create<BatchLogDomain>());

            // returns a year that matches 1 year from GFSs
            var chargesBatchYear = RandomGen.Create<ChargesBatchYearDomain>();
            _mockChargesBatchYearsGateway
                .Setup(cby => cby.GetPendingYear())
                .ReturnsAsync(chargesBatchYear);

            // returns non-empty list of GFSs
            var googleFileSettings = RandomGen.CreateMany<GoogleFileSettingDomain>(quantity: 1).ToList();
            googleFileSettings[0].FileYear = chargesBatchYear.Year;
            _mockGoogleFileSettingGateway
                .Setup(gfs => gfs.GetSettingsByLabel(It.IsAny<string>()))
                .ReturnsAsync(googleFileSettings);

            // We don't want to sit for 40 minutes waiting for all 10 retries.
            var retryCount = failureCount; 

            Action<string, string, string> action = (string _, string _, string _) =>
            {
                if (retryCount > 0)
                {
                    retryCount = retryCount - 1;
                    throw new Exception("Sheets Fetch Failed!");
                }
            };

            _mockGoogleClientService.Setup(
                gc => gc.ReadSheetToEntitiesAsync<ChargesAuxDomain>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )
            )
            .Callback(action)
            .ReturnsAsync(RandomGen.CreateMany<ChargesAuxDomain>().ToList());

            // It's going to be failure count on 1st sheet tab + sheet tab count of successes
            var expectedCallCount = Enum.GetValues(typeof(RentGroup)).Length + failureCount;

            // act
            await _ucWithTestedRetryPolicy.ExecuteAsync();

            // assert
            _mockGoogleClientService.Verify(
                gc => gc.ReadSheetToEntitiesAsync<ChargesAuxDomain>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ), Times.Exactly(expectedCallCount)
            );
        }
    }
}
