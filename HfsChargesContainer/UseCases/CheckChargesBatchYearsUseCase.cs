using HfsChargesContainer.Gateways.Interfaces;
using HfsChargesContainer.UseCases.Interfaces;

namespace HfsChargesContainer.UseCases
{
    public class CheckChargesBatchYearsUseCase : ICheckChargesBatchYearsUseCase
    {
        private readonly IChargesBatchYearsGateway _chargesBatchYearsGateway;
        // TODO: Move the Environment Variable out to start up vvvv
        private readonly string _chargesBatchYears = Environment.GetEnvironmentVariable("CHARGES_BATCH_YEARS");

        public CheckChargesBatchYearsUseCase(IChargesBatchYearsGateway chargesBatchYearsGateway)
        {
            _chargesBatchYearsGateway = chargesBatchYearsGateway;
        }

        public async Task<bool> ExecuteAsync()
        {
            var existDate = await _chargesBatchYearsGateway.ExistDataForToday().ConfigureAwait(false);
            if (!existDate)
            {
                var chargesBatchYears = _chargesBatchYears.Split(';').Select(int.Parse).ToList();
                foreach (var year in chargesBatchYears)
                {
                    await _chargesBatchYearsGateway.CreateAsync(year).ConfigureAwait(false);
                }
            }

            var pendingYear = await _chargesBatchYearsGateway.GetPendingYear().ConfigureAwait(false);
            var continueProcess = pendingYear is not null; 

            return continueProcess;
        }
    }
}
