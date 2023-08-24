using HfsChargesContainer.Domain;
using HfsChargesContainer.Infrastructure.Models;

namespace HfsChargesContainer.Factories
{
    public static class BatchLogErrorFactory
    {
        public static BatchLogErrorDomain ToDomain(this BatchLogError batchLogError)
        {
            if (batchLogError == null)
                return null;

            return new BatchLogErrorDomain
            {
                Id = batchLogError.Id,
                BatchLogId = batchLogError.BatchLogId,
                Type = batchLogError.Type,
                Message = batchLogError.Message,
                Timestamp = batchLogError.Timestamp
            };
        }

        public static List<BatchLogErrorDomain> ToDomain(
            this ICollection<BatchLogError> batchLogError)
        {
            return batchLogError?.Select(b => b.ToDomain()).ToList();
        }
    }
}
