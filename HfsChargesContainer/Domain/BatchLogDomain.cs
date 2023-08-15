namespace HfsChargesContainer.Domain
{
    public class BatchLogDomain
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public bool IsSuccess { get; set; }
        public IList<BatchLogErrorDomain> BatchLogErrors { get; set; }
    }
}
