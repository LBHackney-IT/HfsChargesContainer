using HfsChargesContainer.Domain;

namespace HfsChargesContainer.Gateways.Interfaces
{
    public interface IChargesGateway
    {
        public Task CreateBulkAsync(IList<ChargesAuxDomain> chargesAuxDomain, string rentGroup, int year);
        public Task ClearChargesAuxiliary();
        public Task LoadCharges();
        public Task LoadChargesHistory(int processingYear);
    }
}
