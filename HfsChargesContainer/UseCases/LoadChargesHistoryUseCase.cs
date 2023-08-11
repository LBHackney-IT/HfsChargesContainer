using HfsChargesContainer.Gateways.Interfaces;
using HfsChargesContainer.Helpers;
using HfsChargesContainer.UseCases.Interfaces;

namespace HfsChargesContainer.UseCases
{
    public class LoadChargesHistoryUseCase : ILoadChargesHistoryUseCase
    {
        private readonly IBatchLogGateway _batchLogGateway;
        private readonly IBatchLogErrorGateway _batchLogErrorGateway;
        private readonly IChargesBatchYearsGateway _chargesBatchYearsGateway;
        private readonly IChargesGateway _chargesGateway;
        private readonly string _label = "ChargesHistory";

        public LoadChargesHistoryUseCase(
            IBatchLogGateway batchLogGateway,
            IBatchLogErrorGateway batchLogErrorGateway,
            IChargesBatchYearsGateway chargesBatchYearsGateway,
            IChargesGateway chargesGateway)
        {
            _batchLogGateway = batchLogGateway;
            _batchLogErrorGateway = batchLogErrorGateway;
            _chargesBatchYearsGateway = chargesBatchYearsGateway;
            _chargesGateway = chargesGateway;
        }

        public async Task<bool> ExecuteAsync()
        {

            LoggingHandler.LogInfo($"Starting load charges history");
            var batch = await _batchLogGateway.CreateAsync(_label).ConfigureAwait(false);
            try
            {
                var pendingYear = await _chargesBatchYearsGateway.GetPendingYear().ConfigureAwait(false);

                LoggingHandler.LogInfo($"Load ChargesHistory table (year: {pendingYear.Year})");
                await _chargesGateway.LoadChargesHistory(pendingYear.Year).ConfigureAwait(false);

                await _batchLogGateway.SetToSuccessAsync(batch.Id).ConfigureAwait(false);

                LoggingHandler.LogInfo($"End load charges history");
                // This return is not meaningful, but keeping it due to it having been here historically.
                return true;
            }
            catch (Exception exc)
            {
                var namespaceLabel = $"{nameof(HfsChargesContainer)}.{nameof(ProcessEntryPoint)}.{nameof(ExecuteAsync)}";

                await _batchLogErrorGateway
                    .CreateAsync(batch.Id, "ERROR", $"Application error. Not possible to load charges history")
                    .ConfigureAwait(false);

                LoggingHandler.LogError($"{namespaceLabel} Application error");
                LoggingHandler.LogError(exc.ToString());

                throw;
            }
        }
    }
}
