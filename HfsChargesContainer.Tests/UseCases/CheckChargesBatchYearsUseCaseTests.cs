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
        public async Task ExecuteAsync_OnSunday_CreatesAllBatchYears()
        {
            // arrange
            var sunday = GetNextDayOfWeek(DayOfWeek.Sunday);
            var originalNow = DateTime.Now;
            using (new DateTimeScope(sunday))
            {
                _gatewayMock.Setup(g => g.ExistDataForToday()).ReturnsAsync(false);
                _gatewayMock.Setup(g => g.CreateAsync(It.IsAny<int>(), false)).ReturnsAsync(RandomGen.Create<CBYDomain>());
                _gatewayMock.Setup(g => g.GetPendingYear()).ReturnsAsync(RandomGen.Create<CBYDomain>());

                // act
                await _classUnderTest.ExecuteAsync();

                // assert
                foreach (var year in _batchYears)
                {
                    _gatewayMock.Verify(g => g.CreateAsync(year, false), Times.Once);
                }
                _gatewayMock.Verify(g => g.CreateAsync(It.IsAny<int>(), false), Times.Exactly(_batchYears.Count));
            }
        }

        [Fact]
        public async Task ExecuteAsync_OnNonSunday_CreatesOnlyMaxBatchYear()
        {
            // arrange
            var maxYear = _batchYears.Max();
            var nonSunday = GetNextDayOfWeek(DayOfWeek.Wednesday);

            _gatewayMock.Setup(g => g.ExistDataForToday()).ReturnsAsync(false);
            _gatewayMock.Setup(g => g.CreateAsync(It.IsAny<int>(), false)).ReturnsAsync(RandomGen.Create<CBYDomain>());
            _gatewayMock.Setup(g => g.GetPendingYear()).ReturnsAsync(RandomGen.Create<CBYDomain>());

            // act
            using (new DateTimeScope(nonSunday))
            {
                await _classUnderTest.ExecuteAsync();
            }

            // assert
            _gatewayMock.Verify(g => g.CreateAsync(maxYear, false), Times.Once);
            _gatewayMock.Verify(g => g.CreateAsync(It.IsAny<int>(), false), Times.Once);
        }

        private static DateTime GetNextDayOfWeek(DayOfWeek day)
        {
            var today = DateTime.Now;
            int daysUntil = ((int)day - (int)today.DayOfWeek + 7) % 7;
            return today.AddDays(daysUntil == 0 ? 7 : daysUntil).Date;
        }

        private class DateTimeScope : IDisposable
        {
            private static DateTime? _overrideNow;
            private readonly DateTime? _previous;

            public DateTimeScope(DateTime now)
            {
                _previous = _overrideNow;
                _overrideNow = now;
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(DateTimeNowShim).TypeHandle);
            }

            public void Dispose()
            {
                _overrideNow = _previous;
            }

            public static DateTime Now => _overrideNow ?? DateTime.Now;
        }

        private static class DateTimeNowShim
        {
            static DateTimeNowShim()
            {
                System.Reflection.FieldInfo field = typeof(DateTime).GetField("s_now", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                if (field != null)
                {
                    field.SetValue(null, (Func<DateTime>)(() => DateTimeScope.Now));
                }
            }
        }
    }
}
