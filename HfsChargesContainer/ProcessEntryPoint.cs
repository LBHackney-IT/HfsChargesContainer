using HfsChargesContainer.UseCases.Interfaces;

namespace HfsChargesContainer
{
    public class ProcessEntryPoint : IEntry
    {
        private readonly IUseCase1 _useCase1;
        private readonly ILoadChargesUseCase _loadChargesUseCase;
        private readonly ICheckChargesBatchYearsUseCase _checkChargesBatchYearsUseCase;

        public ProcessEntryPoint(
            IUseCase1 useCase1,
            ILoadChargesUseCase loadChargesUseCase,
            ICheckChargesBatchYearsUseCase checkChargesBatchYearsUseCase
        )
        {
            _useCase1 = useCase1;
            _loadChargesUseCase = loadChargesUseCase;
            _checkChargesBatchYearsUseCase = checkChargesBatchYearsUseCase;
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
        }
    }
}
