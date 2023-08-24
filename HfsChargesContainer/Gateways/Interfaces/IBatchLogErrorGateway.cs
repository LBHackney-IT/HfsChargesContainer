using HfsChargesContainer.Domain;

namespace HfsChargesContainer.Gateways.Interfaces
{
    public interface IBatchLogErrorGateway
    {
        public Task<BatchLogErrorDomain> CreateAsync(long batchId, string type, string message);
    }
}
