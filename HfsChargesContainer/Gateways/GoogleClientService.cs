using System.Dynamic;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using HfsChargesContainer.Gateways.Interfaces;
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

        public async Task<string> ReadSheetAsync(string spreadSheetId, string sheetName, string sheetRange)
        {
            Console.WriteLine($"Executing ReadSheetAsync method with {sheetName}!{sheetRange}.");

            SpreadsheetsResource.ValuesResource.GetRequest getSheetRequest =
                _sheetsService.Spreadsheets.Values.Get(spreadSheetId, $"{sheetName}!{sheetRange}");

            ValueRange response = await getSheetRequest.ExecuteAsync().ConfigureAwait(false);
            IList<IList<object>> values = response.Values;

            if (values == null || !values.Any())
                throw new Exception("Sheet is empty!");

            IList<string> sheetHeaders = values.First().Select(cell => cell.ToString()).ToList();
            IList<object> rowObjects = new List<object>();

            // For each row of actual data
            foreach (var row in values.Skip(1))
            {
                int cellIterator = 0;
                int rowCellCount = row.Count;
                dynamic rowItem = new ExpandoObject();
                var rowItemAccessor = rowItem as IDictionary<string, object>;

                // Add the cell values using the headers as properties
                foreach (string header in sheetHeaders)
                {
                    if (cellIterator < rowCellCount)
                    {
                        string propertyName = string.IsNullOrWhiteSpace(header)
                            ? $"Header{cellIterator}"
                            : header;

                        // Assign the value to this property
                        rowItemAccessor[propertyName] = row[cellIterator++];
                    }
                }
                rowObjects.Add(rowItem);
            }

            return JsonConvert.SerializeObject(rowObjects);
        }
    }
}