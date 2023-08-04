using HfsChargesContainer.Gateways.Interfaces;
using HfsChargesContainer.UseCases.Interfaces;

namespace HfsChargesContainer.UseCases
{
    public class UseCase1 : IUseCase1
    {
        private readonly IHousingFinanceGateway _housingFinanceGateway;

        public UseCase1(IHousingFinanceGateway housingFinanceGateway)
        {
            _housingFinanceGateway = housingFinanceGateway;
        }

        public void Execute()
        {
            Console.WriteLine("Executing UseCase.");

            var wbyCount = _housingFinanceGateway.WeeksByYearCount();
            Console.WriteLine($"Total WBY record count is: {wbyCount}.");

            var topWBYIs = _housingFinanceGateway.GetTopWeekByYear();
            Console.WriteLine($"Year: {topWBYIs.YearNo}, Week: {topWBYIs.WeekNo}, Date: {topWBYIs.StartDate.ToUniversalTime()}");
        }
    }
}
