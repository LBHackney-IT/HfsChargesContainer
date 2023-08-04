using HfsChargesContainer.Infrastructure.Models;

namespace HfsChargesContainer.Gateways.Interfaces
{
    public interface IHousingFinanceGateway
    {
        public int WeeksByYearCount();
        public WeeksByYear GetTopWeekByYear();
    }
}
