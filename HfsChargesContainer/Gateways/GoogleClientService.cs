using System.Dynamic;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using HfsChargesContainer.Gateways.Interfaces;
using HfsChargesContainer.Helpers;
using Newtonsoft.Json;

namespace HfsChargesContainer.Gateways
{
    public class GoogleClientService : IGoogleClientService
    {
        private readonly SheetsService _sheetsService;

        public GoogleClientService(SheetsService sheetService)
        {
            _sheetsService = sheetService;
        }

        public async Task<IList<_TEntity>> ReadSheetToEntitiesAsync<_TEntity>(string spreadSheetId, string sheetName,
            string sheetRange) where _TEntity : class
        {
            SpreadsheetsResource.ValuesResource.GetRequest getter =
                _sheetsService.Spreadsheets.Values.Get(spreadSheetId, $"{sheetName}!{sheetRange}");

            ValueRange response = await getter.ExecuteAsync().ConfigureAwait(false);
            IList<IList<object>> values = response.Values;

            if (values == null || !values.Any())
            {
                LoggingHandler.LogInfo($"No data found. Spreadsheet id: {spreadSheetId}, sheet name: {sheetName}, sheet range: {sheetRange}");
                return null;
            }
            LoggingHandler.LogInfo($"Rows {values.Count} found");

            // Get the headers
            IList<string> headers = values.First().Select(cell => cell.ToString()).ToList();
            IList<object> rowObjects = new List<object>();
            LoggingHandler.LogInfo($"Writing row values to objects, {headers.Count} headers found");

            // For each row of actual data
            foreach (var row in values.Skip(1))
            {
                int cellIterator = 0;
                int rowCellCount = row.Count;
                dynamic rowItem = new ExpandoObject();
                var rowItemAccessor = rowItem as IDictionary<string, object>;

                // Add the cell values using the headers as properties
                foreach (string header in headers)
                {
                    if (cellIterator < rowCellCount)
                    {
                        string propertyName = string.IsNullOrWhiteSpace(header)
                            ? $"Header{cellIterator}"
                            : header;

                        // Assign the value to this property
                        var cellValue = row[cellIterator++];
                        if (cellValue is string stringValue)
                        {
                            cellValue = stringValue.Trim();
                        }
                        rowItemAccessor[propertyName] = cellValue;
                    }
                }
                rowObjects.Add(rowItem);
            }


            try
            {
                LoggingHandler.LogInfo($"Writing values to objects and serializing");
                var entities = new List<_TEntity>();
                bool hasErrors = false;

                for (int i = 0; i < rowObjects.Count; i++)
                {
                    try
                    {
                        string convertedJson = JsonConvert.SerializeObject(rowObjects[i]);
                        var entity = JsonConvert.DeserializeObject<_TEntity>(convertedJson);
                        if (entity != null)
                        {
                            entities.Add(entity);
                        }
                    }
                    catch (Exception exc)
                    {
                        hasErrors = true;
                        int spreadsheetRowNumber = i + 2; // exclude header
                        LoggingHandler.LogWarning($"Skip row: Failure parsing row {spreadsheetRowNumber}. Message: {exc.Message}");
                    }
                }

                if (hasErrors)
                {
                    LoggingHandler.LogError("ALARM: Spreadsheet row parsing failed. Some rows were skipped.");
                }

                return entities;
            }
            catch (Exception exc)
            {
                LoggingHandler.LogInfo($"Failure writing values to objects and serializing");
                LoggingHandler.LogInfo(exc.ToString());

                throw;
            }
        }
    }
}
