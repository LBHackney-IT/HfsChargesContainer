using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HfsChargesContainer.Domain;
using HfsChargesContainer.Infrastructure.Models;

namespace HfsChargesContainer.Factories
{
    public static class ChargesBatchYearsFactory
    {
        public static ChargesBatchYearDomain ToDomain(this ChargesBatchYear chargesBatchYear)
        {
            if (chargesBatchYear == null)
                return null;

            return new ChargesBatchYearDomain
            {
                Id = chargesBatchYear.Id,
                ProcessingDate = chargesBatchYear.ProcessingDate,
                Year = chargesBatchYear.Year,
                IsRead = chargesBatchYear.IsRead
            };
        }

        public static List<ChargesBatchYearDomain> ToDomain(this ICollection<ChargesBatchYear> chargesBatchYear)
        {
            return chargesBatchYear?.Select(b => b.ToDomain()).ToList();
        }
    }
}
