using EFCore.BulkExtensions;
using HfsChargesContainer.Domain;
using HfsChargesContainer.Gateways.Interfaces;
using HfsChargesContainer.Helpers;
using HfsChargesContainer.Infrastructure;

namespace HfsChargesContainer.Gateways
{
    public class ChargesGateway : IChargesGateway
    {
        private readonly DatabaseContext _context;
        // TODO: Move this to Start up

        private readonly int _batchSize = Convert.ToInt32(Environment.GetEnvironmentVariable("BATCH_SIZE"));

        public ChargesGateway(DatabaseContext context)
        {
            _context = context;
        }

        public async Task CreateBulkAsync(IList<ChargesAuxDomain> chargesAuxDomain, string rentGroup, int year)
        {
            try
            {
                var chargesAux = chargesAuxDomain.Select(c => new ChargesAux()
                {
                    PropertyRef = c.PropertyRef,
                    RentGroup = rentGroup,
                    Year = year,
                    DAT = c.DAT ?? 0,
                    DBR = c.DBR ?? 0,
                    DC4 = c.DC4 ?? 0,
                    DC5 = c.DC5 ?? 0,
                    DCB = c.DCB ?? 0,
                    DCC = c.DCC ?? 0,
                    DCE = c.DCE ?? 0,
                    DCI = c.DCI ?? 0,
                    DCO = c.DCO ?? 0,
                    DCP = c.DCP ?? 0,
                    DCT = c.DCT ?? 0,
                    DGA = c.DGA ?? 0,
                    DGM = c.DGM ?? 0,
                    DGR = c.DGR ?? 0,
                    DHA = c.DHA ?? 0,
                    DHE = c.DHE ?? 0,
                    DHM = c.DHM ?? 0,
                    DIN = c.DIN ?? 0,
                    DIT = c.DIT ?? 0,
                    DKF = c.DKF ?? 0,
                    DLL = c.DLL ?? 0,
                    DLP = c.DLP ?? 0,
                    DMC = c.DMC ?? 0,
                    DMJ = c.DMJ ?? 0,
                    DMR = c.DMR ?? 0,
                    DR5 = c.DR5 ?? 0,
                    DRP = c.DRP ?? 0,
                    DRR = c.DRR ?? 0,
                    DSA = c.DSA ?? 0,
                    DSB = c.DSB ?? 0,
                    DSC = c.DSC ?? 0,
                    DSJ = c.DSJ ?? 0,
                    DSO = c.DSO ?? 0,
                    DSR = c.DSR ?? 0,
                    DST = c.DST ?? 0,
                    DTA = c.DTA ?? 0,
                    DTC = c.DTC ?? 0,
                    DTL = c.DTL ?? 0,
                    DTV = c.DTV ?? 0,
                    DVA = c.DVA ?? 0,
                    DWR = c.DWR ?? 0,
                    DWS = c.DWS ?? 0,
                    DWW = c.DWW ?? 0,
                    RCI = c.RCI ?? 0,
                    RPD = c.RPD ?? 0,
                    RSJ = c.RSJ ?? 0,
                    RTM = c.RTM ?? 0,
                    RWA = c.RWA ?? 0,
                    WON = c.WON ?? 0,
                }).ToList();

                await _context.BulkInsertAsync(chargesAux, new BulkConfig { BatchSize = _batchSize }).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                LoggingHandler.LogError(e.Message);
                LoggingHandler.LogError(e.StackTrace);
                throw;
            }
        }

        public async Task ClearChargesAuxiliary()
        {
            try
            {
                await _context.TruncateChargesAuxiliary().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                LoggingHandler.LogError(e.Message);
                LoggingHandler.LogError(e.StackTrace);
                throw;
            }
        }

        public async Task LoadCharges()
        {
            try
            {
                await _context.LoadCharges().ConfigureAwait(false);
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
