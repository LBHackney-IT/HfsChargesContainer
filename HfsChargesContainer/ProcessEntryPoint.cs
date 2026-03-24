using HfsChargesContainer.Domain.Exceptions;
using HfsChargesContainer.Helpers;
using HfsChargesContainer.UseCases.Interfaces;

namespace HfsChargesContainer
{
    public class ProcessEntryPoint : IEntry
    {
        private readonly ILoadChargesUseCase _loadChargesUseCase;
        private readonly ILoadChargesHistoryUseCase _loadChargesHistoryUseCase;
        private readonly ICheckChargesBatchYearsUseCase _checkChargesBatchYearsUseCase;
        private readonly ILoadChargesTransactionsUseCase _loadChargesTransactionsUseCase;

        public ProcessEntryPoint(
            ILoadChargesUseCase loadChargesUseCase,
            ILoadChargesHistoryUseCase loadChargesHistoryUseCase,
            ICheckChargesBatchYearsUseCase checkChargesBatchYearsUseCase,
            ILoadChargesTransactionsUseCase loadChargesTransactionsUseCase
        )
        {
            _loadChargesUseCase = loadChargesUseCase;
            _loadChargesHistoryUseCase = loadChargesHistoryUseCase;
            _checkChargesBatchYearsUseCase = checkChargesBatchYearsUseCase;
            _loadChargesTransactionsUseCase = loadChargesTransactionsUseCase;
        }

        public async Task Run()
        {
            LoggingHandler.LogInfo("Starting the Charges Ingest process.");

            while (await UnprocessedFinancialYearExists())
            {
                await ProcessAFinancialYear();
            }

            LoggingHandler.LogInfo("Charges Ingest process has finished executing.");
        }

        private async Task<bool> UnprocessedFinancialYearExists()
            => await _checkChargesBatchYearsUseCase.ExecuteAsync();

        private async Task ProcessAFinancialYear()
        {
            LoggingHandler.LogInfo("Loading Charges from the Google Sheets.");
            bool chargesWereLoaded = await _loadChargesUseCase.ExecuteAsync();

            if (!chargesWereLoaded)
            {
                var errorMessage = $"GDrive charges data sheets' identifiers were not found or {LoggingHandler.ProcessCompletedSuccessfullyMessage} was not reached!";

                LoggingHandler.LogError(errorMessage);

                throw new ResourceCannotBeFoundException(errorMessage);
            }

            LoggingHandler.LogInfo("Loading Charges to ChargesHistory.");
            bool historyLoaded = await _loadChargesHistoryUseCase.ExecuteAsync();
            if (!historyLoaded)
            {
                var errorMessage = $"Charges History load failed: {LoggingHandler.ProcessCompletedSuccessfullyMessage} was not reached!";
                LoggingHandler.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            LoggingHandler.LogInfo("Inserting new Charge Transactions.");
            bool transactionsLoaded = await _loadChargesTransactionsUseCase.ExecuteAsync();
            if (!transactionsLoaded)
            {
                var errorMessage = $"Charge Transactions load failed: {LoggingHandler.ProcessCompletedSuccessfullyMessage} was not reached!";
                LoggingHandler.LogError(errorMessage);
                throw new Exception(errorMessage);
            }
        }
    }
}
