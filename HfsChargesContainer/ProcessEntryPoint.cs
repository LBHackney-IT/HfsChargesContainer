using HfsChargesContainer.UseCases.Interfaces;

namespace HfsChargesContainer
{
    public class ProcessEntryPoint : IEntry
    {
        private readonly IUseCase1 _useCase1;

        public ProcessEntryPoint(IUseCase1 useCase1)
        {
            _useCase1 = useCase1;
        }
        public async Task Run()
        {
            Console.WriteLine("Process Entry Point!");
            await _useCase1.Execute();
        }
    }
}
