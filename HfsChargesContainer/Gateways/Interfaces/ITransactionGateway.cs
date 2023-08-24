namespace HfsChargesContainer.Gateways.Interfaces
{
    public interface ITransactionGateway
    {
        public Task LoadChargesTransactions(int processingYear);
    }
}
