using System;

namespace HfsChargesContainer.Domain
{
    public class ChargesBatchYearDomain
    {
        public long Id { get; set; }
        public DateTime ProcessingDate { get; set; }
        public int Year { get; set; }
        public bool IsRead { get; set; }
    }
}
