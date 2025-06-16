using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HfsChargesContainer.Gateways.Interfaces;
using HfsChargesContainer.Options;
using HfsChargesContainer.UseCases;
using HfsChargesContainer.UseCases.Interfaces;
using Moq;
using Xunit;
using HfsChargesContainer.Tests.TestsHelpers;

namespace HfsChargesContainer.Tests.UseCases
{
    using CBYDomain = Domain.ChargesBatchYearDomain;

    public class CheckChargesBatchYearsUseCaseTests
    {
        private readonly Mock<IChargesBatchYearsGateway> _gatewayMock;
        private readonly List<int> _batchYears = new() { 2021, 2022, 2023 };
        private readonly ChargesBatchYearsOptions _batchYearOptions;
        private readonly ICheckChargesBatchYearsUseCase _classUnderTest;

        public CheckChargesBatchYearsUseCaseTests()
        {
            _gatewayMock = new Mock<IChargesBatchYearsGateway>();
            _batchYearOptions = new ChargesBatchYearsOptions(string.Join(";", _batchYears));
            _classUnderTest = new CheckChargesBatchYearsUseCase(_gatewayMock.Object, _batchYearOptions);
        }

        [Fact]
        public async Task OnSundayAllBatchYearsGetProcessed()
        {
            // arrange
            var sunday = GetNextSpecifiedDayOfWeek(DayOfWeek.Sunday);

            _gatewayMock.Setup(g => g.ExistDataForToday()).ReturnsAsync(false);
            _gatewayMock.Setup(g => g.CreateAsync(It.IsAny<int>(), false)).ReturnsAsync(RandomGen.Create<CBYDomain>());
            _gatewayMock.Setup(g => g.GetPendingYear()).ReturnsAsync(RandomGen.Create<CBYDomain>());

            // act
            using (new DateTimeContext(sunday))
            {
                await _classUnderTest.ExecuteAsync();
            }

            // assert
            _gatewayMock.Verify(g => g.CreateAsync(It.IsAny<int>(), false), Times.Exactly(_batchYears.Count));

            foreach (var year in _batchYears)
            {
                _gatewayMock.Verify(g => g.CreateAsync(year, false), Times.Once);
            }
        }

        [Fact(Skip = "Test ignored to resume Sunday processing")]
        public async Task OnNonSundayOnlyLatestBatchYearGetProcessed()
        {
            // arrange
            var maxYear = _batchYears.Max();
            var nonSunday = GetNextSpecifiedDayOfWeek(DayOfWeek.Wednesday);

            _gatewayMock.Setup(g => g.ExistDataForToday()).ReturnsAsync(false);
            _gatewayMock.Setup(g => g.CreateAsync(It.IsAny<int>(), false)).ReturnsAsync(RandomGen.Create<CBYDomain>());
            _gatewayMock.Setup(g => g.GetPendingYear()).ReturnsAsync(RandomGen.Create<CBYDomain>());

            // act
            using (new DateTimeContext(nonSunday))
            {
                await _classUnderTest.ExecuteAsync();
            }

            // assert
            _gatewayMock.Verify(g => g.CreateAsync(maxYear, false), Times.Once);
            _gatewayMock.Verify(g => g.CreateAsync(It.IsAny<int>(), false), Times.Once);
        }

        private static DateTime GetNextSpecifiedDayOfWeek(DayOfWeek day)
        {
            var referenceDate = DateTime.Now;
            int daysUntil = ((int)day - (int)referenceDate.DayOfWeek + 7) % 7;
            return referenceDate.AddDays(daysUntil).Date;
        }
    }
}
