using HfsChargesContainer.Gateways.Interfaces;
using HfsChargesContainer.Helpers;
using HfsChargesContainer.Options;
using HfsChargesContainer.UseCases.Interfaces;

namespace HfsChargesContainer.UseCases
{
    public class CheckChargesBatchYearsUseCase : ICheckChargesBatchYearsUseCase
    {
        private readonly IChargesBatchYearsGateway _chargesBatchYearsGateway;
        private readonly string _chargesBatchYears;

        public CheckChargesBatchYearsUseCase(
            IChargesBatchYearsGateway chargesBatchYearsGateway,
            ChargesBatchYearsOptions chargesBatchYearsOptions
        )
        {
            _chargesBatchYearsGateway = chargesBatchYearsGateway;
            _chargesBatchYears = chargesBatchYearsOptions.ChargesBatchYears;
        }

        public async Task<bool> ExecuteAsync()
        {
            var existDate = await _chargesBatchYearsGateway.ExistDataForToday().ConfigureAwait(false);
            if (!existDate)
            {
                var chargesBatchYears = _chargesBatchYears.Split(';').Select(int.Parse).ToList();
                int maxYear = chargesBatchYears.Max();

                if (DateTimeProvider.Now.DayOfWeek != DayOfWeek.Sunday)
                    chargesBatchYears = chargesBatchYears.Where(year => year == maxYear).ToList();

                foreach (var year in chargesBatchYears)
                {
                    await _chargesBatchYearsGateway.CreateAsync(year).ConfigureAwait(false);
                }
            }

            var pendingYear = await _chargesBatchYearsGateway.GetPendingYear().ConfigureAwait(false);
            var continueProcess = pendingYear is not null;

            LoggingHandler.LogInfo(continueProcess
                ? $"Starting to process the: {pendingYear.Year} Financial year."
                : $"All the today's pending financial years have already been processed."
            );

            return continueProcess;
        }
    }
}
