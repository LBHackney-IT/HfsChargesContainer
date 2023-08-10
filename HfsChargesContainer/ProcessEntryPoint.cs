using HfsChargesContainer.UseCases.Interfaces;

namespace HfsChargesContainer
{
    public class ProcessEntryPoint : IEntry
    {
        private readonly IUseCase1 _useCase1;
        private readonly ICheckChargesBatchYearsUseCase _checkChargesBatchYearsUseCase;

        public ProcessEntryPoint(
            IUseCase1 useCase1,
            ICheckChargesBatchYearsUseCase checkChargesBatchYearsUseCase
        )
        {
            _useCase1 = useCase1;
            _checkChargesBatchYearsUseCase = checkChargesBatchYearsUseCase;
        }
        public async Task Run()
        {
            Console.WriteLine("Process Entry Point!");
            Console.WriteLine("Executing UC1.");
            await _useCase1.Execute();
            Console.WriteLine("Executing CheckChargesBatchYearsUC.");
            var canBeContinued = await _checkChargesBatchYearsUseCase.ExecuteAsync();
            Console.WriteLine($"Continue? {canBeContinued}");
        }
    }
}
