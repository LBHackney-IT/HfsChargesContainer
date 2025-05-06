using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HfsChargesContainer.Domain;
using HfsChargesContainer.Gateways.Interfaces;
using HfsChargesContainer.UseCases;
using HfsChargesContainer.UseCases.Interfaces;
using Moq;
using Polly;
using Xunit;

namespace HfsChargesContainer.Tests.UseCases
{
    using FetchChargesSheet = Func<Task<IList<ChargesAuxDomain>>>;
    using ReadGSheetInputsTuple = ValueTuple<string, string, string>;

    public class LoadChargesUseCaseTests
    {
        Mock<IBatchLogGateway> _mockBatchLogGateway;
        Mock<IBatchLogErrorGateway> _mockBatchLogErrorGateway;
        Mock<IChargesBatchYearsGateway> _mockChargesBatchYearsGateway;
        Mock<IChargesGateway> _mockChargesGateway;
        Mock<IGoogleFileSettingGateway> _mockGoogleFileSettingGateway;
        Mock<IGoogleClientService> _mockGoogleClientService;
        Mock<IAsyncPolicy<IList<ChargesAuxDomain>>> _mockFetchSheetRetryPolicy;
        ILoadChargesUseCase _classUnderTest;

        public LoadChargesUseCaseTests()
        {
            _mockBatchLogGateway = new Mock<IBatchLogGateway>();
            _mockBatchLogErrorGateway = new Mock<IBatchLogErrorGateway>();
            _mockChargesBatchYearsGateway = new Mock<IChargesBatchYearsGateway>();
            _mockChargesGateway = new Mock<IChargesGateway>();
            _mockGoogleFileSettingGateway = new Mock<IGoogleFileSettingGateway>();
            _mockGoogleClientService = new Mock<IGoogleClientService>();
            _mockFetchSheetRetryPolicy = new Mock<IAsyncPolicy<IList<ChargesAuxDomain>>>();

            _classUnderTest = new LoadChargesUseCase(
                _mockBatchLogGateway.Object,
                _mockBatchLogErrorGateway.Object,
                _mockChargesBatchYearsGateway.Object,
                _mockChargesGateway.Object,
                _mockGoogleFileSettingGateway.Object,
                _mockGoogleClientService.Object,
                _mockFetchSheetRetryPolicy.Object
            );
        }

        [Fact]
        public async Task LoadChargesUseCaseCallsTheRetrySheetsPolicyWrapperForEachRentGroupTabWithinTheSheet()
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

            // _mockFetchSheetRetryPolicy returns charges
            _mockFetchSheetRetryPolicy
                .Setup(p => p.ExecuteAsync(It.IsAny<FetchChargesSheet>()))
                .ReturnsAsync(RandomGen.CreateMany<ChargesAuxDomain>().ToList());

            var expectedCallCount = Enum.GetValues(typeof(RentGroup)).Length;

            // act
            await _classUnderTest.ExecuteAsync();

            // assert
            _mockFetchSheetRetryPolicy.Verify(
                p => p.ExecuteAsync(
                    It.IsAny<FetchChargesSheet>()
                ),
                Times.Exactly(expectedCallCount)
            );
        }

        [Fact]
        public async Task LoadChargesUseCaseCallsToTheRetrySheetsPolicyWrapperResultInCallsToTheGoogleClientService()
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

            _mockGoogleClientService.Setup(
                gc => gc.ReadSheetToEntitiesAsync<ChargesAuxDomain>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )
            ).ReturnsAsync(RandomGen.CreateMany<ChargesAuxDomain>().ToList());

            // obviously, this assumes that the callback is being called. But we're invoking
            // it with this assumption regardless to see whether the callback itself behaves
            // correctly. See the google client service mock assertion.
            _mockFetchSheetRetryPolicy
                .Setup(
                    p => p.ExecuteAsync(It.IsAny<FetchChargesSheet>())
                )
                .Returns(
                    (FetchChargesSheet incFunc) => Task.FromResult(incFunc().Result)
                );

            var expectedCallCount = Enum.GetValues(typeof(RentGroup)).Length;

            // act
            await _classUnderTest.ExecuteAsync();

            // assert
            _mockFetchSheetRetryPolicy.Verify(
                p => p.ExecuteAsync(
                    It.IsAny<FetchChargesSheet>()
                ),
                Times.Exactly(expectedCallCount)
            );

            _mockGoogleClientService.Verify(
                gc => gc.ReadSheetToEntitiesAsync<ChargesAuxDomain>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ), Times.Exactly(expectedCallCount)
            );
        }

        [Fact]
        public async Task LoadChargesUseCaseGivesToTheRetrySheetsPolicyWrapperAGetChargesSheetCallbackWithTheExpectedParameters()
        {
            // arrange
            var sheetRentGroups = (Enum.GetValues(typeof(RentGroup)) as RentGroup[])
                .Select(srg => srg.ToString())
                .ToList();

            _mockBatchLogGateway
                .Setup(bl => bl.CreateAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(RandomGen.Create<BatchLogDomain>());

            var chargesBatchYear = RandomGen.Create<ChargesBatchYearDomain>();
            _mockChargesBatchYearsGateway
                .Setup(cby => cby.GetPendingYear())
                .ReturnsAsync(chargesBatchYear);

            var googleFileSettings = RandomGen.CreateMany<GoogleFileSettingDomain>(quantity: 1).ToList();
            googleFileSettings[0].FileYear = chargesBatchYear.Year;
            _mockGoogleFileSettingGateway
                .Setup(gfs => gfs.GetSettingsByLabel(It.IsAny<string>()))
                .ReturnsAsync(googleFileSettings);

            // Make the retry policy merely be an empty wrapper around the callback.
            _mockFetchSheetRetryPolicy
                .Setup(
                    p => p.ExecuteAsync(It.IsAny<FetchChargesSheet>())
                )
                .Returns(
                    (FetchChargesSheet incFunc) => Task.FromResult(incFunc().Result)
                );

            var capturedInputs = new List<ReadGSheetInputsTuple>();
            _mockGoogleClientService.Setup(
                gc => gc.ReadSheetToEntitiesAsync<ChargesAuxDomain>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )
            )
            .Callback<string, string, string>((sheetId, sheetName, cellRange) =>
            {
                capturedInputs.Add((sheetId, sheetName, cellRange));
            })
            .ReturnsAsync(RandomGen.CreateMany<ChargesAuxDomain>().ToList());

            // act
            await _classUnderTest.ExecuteAsync();

            // assert
            foreach (var (sheetId, sheetName, cellRange) in capturedInputs)
            {
                var expectedRange = IsSheetTabLeasehold(sheetName)
                    ? "A:AZ"
                    : "A:AX";

                Assert.Equal(sheetId, googleFileSettings.First().GoogleIdentifier);
                Assert.True(sheetRentGroups.Contains(sheetName));
                Assert.Equal(cellRange, expectedRange);
            }
        }

        private bool IsSheetTabLeasehold(string sheetRentGroup)
        {
            return sheetRentGroup == RentGroup.LHServCharges.ToString()
                || sheetRentGroup == RentGroup.LHMajorWorks.ToString();
        }
    }
}
