using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HfsChargesContainer.Domain;

namespace HfsChargesContainer.Gateways.Interfaces
{
    public interface IChargesBatchYearsGateway
    {
        public Task<ChargesBatchYearDomain> CreateAsync(int year, bool isRead = false);
        public Task<bool> SetToSuccessAsync(int year);
        public Task<bool> ExistDataForToday();
        public Task<ChargesBatchYearDomain> GetPendingYear();
    }
}
