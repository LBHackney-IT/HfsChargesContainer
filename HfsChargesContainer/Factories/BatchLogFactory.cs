using HfsChargesContainer.Domain;
using HfsChargesContainer.Infrastructure.Models;

namespace HfsChargesContainer.Factories
{
    public static class BatchLogFactory
    {
        public static BatchLogDomain ToDomain(this BatchLog batchLog)
        {
            if (batchLog == null)
                return null;

            return new BatchLogDomain
            {
                Id = batchLog.Id,
                Type = batchLog.Type,
                StartTime = batchLog.StartTime,
                EndTime = batchLog.EndTime,
                IsSuccess = batchLog.IsSuccess
            };
        }

        public static List<BatchLogDomain> ToDomain(
            this ICollection<BatchLog> batchLog)
        {
            return batchLog?.Select(b => b.ToDomain()).ToList();
        }
    }
}
