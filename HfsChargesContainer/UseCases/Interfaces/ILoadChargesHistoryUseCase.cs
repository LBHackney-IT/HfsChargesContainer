namespace HfsChargesContainer.UseCases.Interfaces
{
    public interface ILoadChargesHistoryUseCase
    {
        public Task<bool> ExecuteAsync();
    }
}
