namespace HfsChargesContainer.UseCases.Interfaces
{
    public interface ILoadChargesTransactionsUseCase
    {
        public Task<bool> ExecuteAsync();
    }
}
