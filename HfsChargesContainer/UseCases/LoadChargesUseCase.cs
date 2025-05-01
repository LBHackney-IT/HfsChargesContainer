using HfsChargesContainer.Domain;
using HfsChargesContainer.Gateways.Interfaces;
using HfsChargesContainer.Helpers;
using HfsChargesContainer.UseCases.Interfaces;
using Polly;

namespace HfsChargesContainer.UseCases
{
    using FetchChargesBySheetTab = Func<Task<IList<ChargesAuxDomain>>>;

    public class LoadChargesUseCase : ILoadChargesUseCase
    {
        private readonly IBatchLogGateway _batchLogGateway;
        private readonly IBatchLogErrorGateway _batchLogErrorGateway;
        private readonly IChargesBatchYearsGateway _chargesBatchYearsGateway;
        private readonly IChargesGateway _chargesGateway;
        private readonly IGoogleFileSettingGateway _googleFileSettingGateway;
        private readonly IGoogleClientService _googleClientService;
        private readonly IAsyncPolicy<IList<ChargesAuxDomain>> _fetchSheetRetryPolicy;
        private const string ChargesLabel = "Charges";

        public LoadChargesUseCase(
            IBatchLogGateway batchLogGateway,
            IBatchLogErrorGateway batchLogErrorGateway,
            IChargesBatchYearsGateway chargesBatchYearsGateway,
            IChargesGateway chargesGateway,
            IGoogleFileSettingGateway googleFileSettingGateway,
            IGoogleClientService googleClientService,
            IAsyncPolicy<IList<ChargesAuxDomain>> fetchSheetRetryPolicy
        )
        {
            _batchLogGateway = batchLogGateway;
            _batchLogErrorGateway = batchLogErrorGateway;
            _chargesBatchYearsGateway = chargesBatchYearsGateway;
            _chargesGateway = chargesGateway;
            _googleFileSettingGateway = googleFileSettingGateway;
            _googleClientService = googleClientService;
            _fetchSheetRetryPolicy = fetchSheetRetryPolicy;
        }

        public async Task<bool> ExecuteAsync()
        {
            LoggingHandler.LogInfo($"Starting charges import");

            var batch = await _batchLogGateway.CreateAsync(ChargesLabel).ConfigureAwait(false);
            var googleFileSettings = await GetGoogleFileSetting(ChargesLabel).ConfigureAwait(false);

            if (!googleFileSettings.Any())
                return false;

            var pendingYear = await _chargesBatchYearsGateway.GetPendingYear().ConfigureAwait(false);

            foreach (var googleFile in googleFileSettings)
            {
                if (googleFile.FileYear == pendingYear.Year)
                {
                    foreach (RentGroup sheetName in Enum.GetValues(typeof(RentGroup)))
                    {
                        var fetchChargesCallback = GetChargesBySheetTabCB(
                            sheetId: googleFile.GoogleIdentifier,
                            tabName: GetSheetRangeByRentGroup(sheetName),
                            cellRange: sheetRange
                        );

                        var chargesAux = await _fetchSheetRetryPolicy
                            .ExecuteAsync(fetchChargesCallback);

                        if (!chargesAux.Any())
                        {
                            LoggingHandler.LogInfo($"No charges data to import. Sheet name: {sheetName}");
                            continue;
                        }

                        await HandleSpreadSheet(batch.Id, chargesAux, sheetName.ToString(), (int)googleFile.FileYear).ConfigureAwait(false);
                    }
                }
            }

            await _batchLogGateway.SetToSuccessAsync(batch.Id).ConfigureAwait(false);
            LoggingHandler.LogInfo($"End charges import");
            return true;
        }

        private string GetSheetRangeByRentGroup(RentGroup sheetTabName)
        {
            const string sheetRangeRent = "A:AX";
            const string sheetRangeLeasehold = "A:AZ";
            const RentGroup[] leaseholdSheetNames = new[] { RentGroup.LHServCharges, RentGroup.LHMajorWorks };
            return leaseholdSheetNames.Contains(sheetTabName) ? sheetRangeLeasehold : sheetRangeRent;
        }

        private FetchChargesBySheetTab GetChargesBySheetTabCB(string sheetId, string tabName, string cellRange)
        {
            return async () => await _googleClientService
                    .ReadSheetToEntitiesAsync<ChargesAuxDomain>(sheetId, tabName, cellRange);
        }

        private async Task<List<GoogleFileSettingDomain>> GetGoogleFileSetting(string label)
        {
            LoggingHandler.LogInfo($"Getting Google file setting for '{label}' label");
            var googleFileSettings = await _googleFileSettingGateway.GetSettingsByLabel(label).ConfigureAwait(false);
            LoggingHandler.LogInfo($"{googleFileSettings.Count} Google file settings found");

            return googleFileSettings;
        }

        private async Task HandleSpreadSheet(long batchId, IList<ChargesAuxDomain> chargesAux, string rentGroup, int year)
        {
            try
            {
                LoggingHandler.LogInfo($"Clear aux table");
                await _chargesGateway.ClearChargesAuxiliary().ConfigureAwait(false);

                LoggingHandler.LogInfo($"Starting bulk insert");
                await _chargesGateway.CreateBulkAsync(chargesAux, rentGroup, year).ConfigureAwait(false);

                LoggingHandler.LogInfo($"Starting merge charges");
                await _chargesGateway.LoadCharges().ConfigureAwait(false);

                LoggingHandler.LogInfo("File success");
            }
            catch (Exception exc)
            {
                var namespaceLabel = $"{nameof(HfsChargesContainer)}.{nameof(ProcessEntryPoint)}.{nameof(HandleSpreadSheet)}";

                await _batchLogErrorGateway.CreateAsync(batchId, ChargesLabel, $"Application error. Not possible to load charges").ConfigureAwait(false);

                LoggingHandler.LogError($"{namespaceLabel} Application error");
                LoggingHandler.LogError(exc.ToString());

                throw;
            }
        }
    }
}
