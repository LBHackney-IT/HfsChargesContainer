using HfsChargesContainer.Gateways.Interfaces;
using HfsChargesContainer.Helpers;
using HfsChargesContainer.UseCases.Interfaces;

namespace HfsChargesContainer.UseCases
{
    public class LoadChargesTransactionsUseCase : ILoadChargesTransactionsUseCase
    {
        private readonly IBatchLogGateway _batchLogGateway;
        private readonly IBatchLogErrorGateway _batchLogErrorGateway;
        private readonly IChargesBatchYearsGateway _chargesBatchYearsGateway;
        private readonly IChargesGateway _chargesGateway;
        private readonly ITransactionGateway _transactionGateway;
        private readonly string _label = "ChargesTransaction";

        public LoadChargesTransactionsUseCase(
            IBatchLogGateway batchLogGateway,
            IBatchLogErrorGateway batchLogErrorGateway,
            IChargesBatchYearsGateway chargesBatchYearsGateway,
            IChargesGateway chargesGateway,
            ITransactionGateway transactionGateway)
        {
            _batchLogGateway = batchLogGateway;
            _batchLogErrorGateway = batchLogErrorGateway;
            _chargesBatchYearsGateway = chargesBatchYearsGateway;
            _chargesGateway = chargesGateway;
            _transactionGateway = transactionGateway;
        }

        public async Task<bool> ExecuteAsync()
        {
            LoggingHandler.LogInfo($"Starting charges transactions import");
            var batch = await _batchLogGateway.CreateAsync(_label).ConfigureAwait(false);
            try
            {
                var pendingYear = await _chargesBatchYearsGateway.GetPendingYear().ConfigureAwait(false);

                LoggingHandler.LogInfo($"Convert ChargesHistory in Transactions");
                await _transactionGateway.LoadChargesTransactions(pendingYear.Year).ConfigureAwait(false);

                await _chargesBatchYearsGateway.SetToSuccessAsync(pendingYear.Year).ConfigureAwait(false);
                await _batchLogGateway.SetToSuccessAsync(batch.Id).ConfigureAwait(false);
                LoggingHandler.LogInfo($"End charges transactions import");
                return true;
            }
            catch (Exception exc)
            {
                var namespaceLabel = $"{nameof(HfsChargesContainer)}.{nameof(ProcessEntryPoint)}.{nameof(ExecuteAsync)}";

                await _batchLogErrorGateway
                    .CreateAsync(batch.Id, "ERROR", $"Application error. Not possible to load charges transactions")
                    .ConfigureAwait(false);

                LoggingHandler.LogError($"{namespaceLabel} Application error");
                LoggingHandler.LogError(exc.ToString());

                throw;
            }
        }
    }
}
