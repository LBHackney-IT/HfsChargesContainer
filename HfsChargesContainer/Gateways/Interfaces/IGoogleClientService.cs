using System.Threading.Tasks;

namespace HfsChargesContainer.Gateways.Interfaces
{
    public interface IGoogleClientService
    {
        public Task<string> ReadSheetAsync(string spreadSheetId, string sheetName, string sheetRange);
    }
}
