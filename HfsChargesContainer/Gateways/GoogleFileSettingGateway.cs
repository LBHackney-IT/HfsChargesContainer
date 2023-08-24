using HfsChargesContainer.Domain;
using HfsChargesContainer.Factories;
using HfsChargesContainer.Gateways.Interfaces;
using HfsChargesContainer.Helpers;
using HfsChargesContainer.Infrastructure;

namespace HfsChargesContainer.Gateways
{
    public class GoogleFileSettingGateway : IGoogleFileSettingGateway
    {

        private readonly DatabaseContext _context;

        public GoogleFileSettingGateway(DatabaseContext context)
        {
            _context = context;
        }

        public Task<List<GoogleFileSettingDomain>> GetSettingsByLabel(string label)
        {
            try
            {
                var googleFileSettings = _context.GoogleFileSettings
                    .Where(item => item.Label.Equals(label)).ToList();
                return Task.FromResult(googleFileSettings.ToDomain());
            }
            catch (Exception e)
            {
                LoggingHandler.LogError(e.Message);
                LoggingHandler.LogError(e.StackTrace);
                throw;
            }
        }
    }
}
