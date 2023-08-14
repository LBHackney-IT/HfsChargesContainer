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
            
            await _loadChargesHistoryUseCase.ExecuteAsync();
            Console.WriteLine("Executing LoadChargesTransactionsUC.");
            await _loadChargesTransactionsUseCase.ExecuteAsync();
        }
    }
}
