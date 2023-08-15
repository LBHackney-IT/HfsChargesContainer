using HfsChargesContainer.Gateways.Interfaces;
using HfsChargesContainer.Helpers;
using HfsChargesContainer.Infrastructure;

namespace HfsChargesContainer.Gateways
{
    public class TransactionGateway : ITransactionGateway
    {

        private readonly DatabaseContext _context;

        public TransactionGateway(DatabaseContext context)
        {
            _context = context;
        }

        public async Task LoadChargesTransactions(int processingYear)
        {
            try
            {
                await _context.LoadChargesTransactions(processingYear).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                LoggingHandler.LogError(e.Message);
                LoggingHandler.LogError(e.StackTrace);
                throw;
            }
        }
    }
}
