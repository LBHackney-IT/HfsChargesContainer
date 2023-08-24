namespace HfsChargesContainer.Domain
{
    public class GoogleFileSettingDomain
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public int FileYear { get; set; }
        public string GoogleIdentifier { get; set; }
        public string FileType { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public bool IsActive => !EndDate.HasValue || EndDate.Value > DateTimeOffset.UtcNow;
    }
}
