using HfsChargesContainer.Domain;

namespace HfsChargesContainer.Gateways.Interfaces
{
    public interface IGoogleFileSettingGateway
    {
        Task<List<GoogleFileSettingDomain>> GetSettingsByLabel(string label);
    }
}
