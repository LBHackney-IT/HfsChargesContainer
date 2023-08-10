using HfsChargesContainer.Domain;

namespace HfsChargesContainer.Gateways.Interfaces
{
    public interface IBatchLogGateway
    {
        public Task<BatchLogDomain> CreateAsync(string type, bool isSuccess = false);
        public Task<bool> SetToSuccessAsync(long batchId);
    }
}
