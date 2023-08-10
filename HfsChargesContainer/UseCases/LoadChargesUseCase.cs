using HfsChargesContainer.Domain;
using HfsChargesContainer.Gateways.Interfaces;
using HfsChargesContainer.Helpers;
using HfsChargesContainer.UseCases.Interfaces;

namespace HfsChargesContainer.UseCases
{
    public class LoadChargesUseCase : ILoadChargesUseCase
    {
        private readonly IBatchLogGateway _batchLogGateway;
        private readonly IBatchLogErrorGateway _batchLogErrorGateway;
        private readonly IChargesBatchYearsGateway _chargesBatchYearsGateway;
        private readonly IChargesGateway _chargesGateway;
        private readonly IGoogleFileSettingGateway _googleFileSettingGateway;
        private readonly IGoogleClientService _googleClientService;
        private const string ChargesLabel = "Charges";

        public LoadChargesUseCase(
            IBatchLogGateway batchLogGateway,
            IBatchLogErrorGateway batchLogErrorGateway,
            IChargesBatchYearsGateway chargesBatchYearsGateway,
            IChargesGateway chargesGateway,
            IGoogleFileSettingGateway googleFileSettingGateway,
            IGoogleClientService googleClientService)
        {
            _batchLogGateway = batchLogGateway;
            _batchLogErrorGateway = batchLogErrorGateway;
            _chargesBatchYearsGateway = chargesBatchYearsGateway;
            _chargesGateway = chargesGateway;
            _googleFileSettingGateway = googleFileSettingGateway;
            _googleClientService = googleClientService;
        }

        public async Task<bool> ExecuteAsync()
        {
            LoggingHandler.LogInfo($"Starting charges import");

            const string sheetRange = "A:AX";

            var batch = await _batchLogGateway.CreateAsync(ChargesLabel).ConfigureAwait(false);
            var googleFileSettings = await GetGoogleFileSetting(ChargesLabel).ConfigureAwait(false);

            if (!googleFileSettings.Any())
                return false;

            var pendingYear = await _chargesBatchYearsGateway.GetPendingYear().ConfigureAwait(false);

            foreach (var googleFile in googleFileSettings)
            {
                if (googleFile.FileYear == pendingYear.Year)
                {
                    foreach (var sheetName in Enum.GetValues(typeof(RentGroup)))
                    {
                        var chargesAux = await _googleClientService
                            .ReadSheetToEntitiesAsync<ChargesAuxDomain>(googleFile.GoogleIdentifier, sheetName.ToString(), sheetRange)
                            .ConfigureAwait(false);

                        if (!chargesAux.Any())
                        {
                            LoggingHandler.LogInfo($"No charges data to import. Sheet name: {sheetName}");
                            continue;
                        }

                        await HandleSpreadSheet(batch.Id, chargesAux, sheetName.ToString(), (int) googleFile.FileYear).ConfigureAwait(false);
                    }
                }
            }

            await _batchLogGateway.SetToSuccessAsync(batch.Id).ConfigureAwait(false);
            LoggingHandler.LogInfo($"End charges import");
            return true;
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
