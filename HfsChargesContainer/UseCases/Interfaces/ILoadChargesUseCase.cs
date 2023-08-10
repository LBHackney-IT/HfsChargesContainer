namespace HfsChargesContainer.UseCases.Interfaces
{
    public interface ILoadChargesUseCase
    {
        public Task<bool> ExecuteAsync();
    }
}
