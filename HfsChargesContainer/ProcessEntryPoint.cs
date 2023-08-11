using HfsChargesContainer.UseCases.Interfaces;

namespace HfsChargesContainer
{
    public class ProcessEntryPoint : IEntry
    {
        private readonly IUseCase1 _useCase1;
        private readonly ILoadChargesUseCase _loadChargesUseCase;
        private readonly ILoadChargesHistoryUseCase _loadChargesHistoryUseCase;
        private readonly ICheckChargesBatchYearsUseCase _checkChargesBatchYearsUseCase;
        private readonly ILoadChargesTransactionsUseCase _loadChargesTransactionsUseCase;

        public ProcessEntryPoint(
            IUseCase1 useCase1,
            ILoadChargesUseCase loadChargesUseCase,
            ILoadChargesHistoryUseCase loadChargesHistoryUseCase,
            ICheckChargesBatchYearsUseCase checkChargesBatchYearsUseCase,
            ILoadChargesTransactionsUseCase loadChargesTransactionsUseCase
        )
        {
            _useCase1 = useCase1;
            _loadChargesUseCase = loadChargesUseCase;
            _loadChargesHistoryUseCase = loadChargesHistoryUseCase;
            _checkChargesBatchYearsUseCase = checkChargesBatchYearsUseCase;
            _loadChargesTransactionsUseCase = loadChargesTransactionsUseCase;
        }
        public async Task Run()
        {
            Console.WriteLine("Process Entry Point!");
            Console.WriteLine("Executing UC1.");
            await _useCase1.Execute();
            Console.WriteLine("Executing CheckChargesBatchYearsUC.");
            var continueToChargesIngest = await _checkChargesBatchYearsUseCase.ExecuteAsync();
            Console.WriteLine($"Continue to CI? {continueToChargesIngest}");
            Console.WriteLine("Executing LoadChargesUC.");
            var continueToChargesHistory = await _loadChargesUseCase.ExecuteAsync();
            Console.WriteLine($"Continue to CH? {continueToChargesHistory}");
            Console.WriteLine("Executing LoadChargesHistoryUC.");
            await _loadChargesHistoryUseCase.ExecuteAsync();
            Console.WriteLine("Executing LoadChargesTransactionsUC.");
            await _loadChargesTransactionsUseCase.ExecuteAsync();
        }
    }
}
