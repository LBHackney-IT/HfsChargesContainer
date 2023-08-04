using HfsChargesContainer.Gateways.Interfaces;
using HfsChargesContainer.Infrastructure;
using HfsChargesContainer.Infrastructure.Models;

namespace HfsChargesContainer.Gateways
{
    public class HousingFinanceGateway : IHousingFinanceGateway
    {
        private readonly DatabaseContext _databaseContext;

        public HousingFinanceGateway(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public int WeeksByYearCount()
        {
            Console.WriteLine("Executing Gateway Count Method.");
            return _databaseContext.WeeksByYear.Count();
        }
        public WeeksByYear GetTopWeekByYear()
        {
            Console.WriteLine("Executing Gateway Top WBY method.");
            return _databaseContext.WeeksByYear.First();
        }
    }
}