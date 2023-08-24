namespace HfsChargesContainer.UseCases.Interfaces
{
    public interface ICheckChargesBatchYearsUseCase
    {
        public Task<bool> ExecuteAsync();
    }
}
