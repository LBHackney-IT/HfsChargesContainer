namespace HfsChargesContainer.Gateways.Interfaces
{
    public interface IGoogleClientService
    {
        public Task<string> ReadSheetAsync(string spreadSheetId, string sheetName, string sheetRange);

        public Task<IList<_TEntity>> ReadSheetToEntitiesAsync<_TEntity>(string spreadSheetId, string sheetName,
            string sheetRange) where _TEntity : class;
    }
}
