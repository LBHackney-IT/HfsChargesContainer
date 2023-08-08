using HfsChargesContainer.Gateways.Interfaces;
using HfsChargesContainer.UseCases.Interfaces;

namespace HfsChargesContainer.UseCases
{
    public class UseCase1 : IUseCase1
    {
        private readonly IHousingFinanceGateway _housingFinanceGateway;
        private readonly IGoogleClientService _googleClientService;

        public UseCase1(IHousingFinanceGateway housingFinanceGateway, IGoogleClientService googleClientService)
        {
            _housingFinanceGateway = housingFinanceGateway;
            _googleClientService = googleClientService;
        }

        public async Task Execute()
        {
            Console.WriteLine("Executing UseCase.");

            var wbyCount = _housingFinanceGateway.WeeksByYearCount();
            Console.WriteLine($"Total WBY record count is: {wbyCount}.");

            Console.WriteLine("Reading a GSheet.");
            // If from URL of a sheet on gdrive. NOT A SECRET!
            var mySheetId = "1qeBOg2oaqBy7GP9faFLPuZIEvoSkJGKltAVb1KjJ0Sk";
            var sheetJson = await _googleClientService.ReadSheetAsync(mySheetId, "Sheet1", "A:AX");
            Console.WriteLine(sheetJson);
        }
    }
}
