using System.ComponentModel;
using System.Text.Json.Serialization;

namespace HfsChargesContainer.Domain
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RentGroup
    {
        [Description("Gar & Park HRA")]
        GarParkHRA,
        [Description("Housing Gen Fund")]
        HousingGenFund,
        [Description("Housing Revenue")]
        HousingRevenue,
        [Description("LH Major Works")]
        LHMajorWorks,
        [Description("LH Serv Charges")]
        LHServCharges,
        [Description("Temp Acc Gen Fun")]
        TempAccGenFun,
        [Description("Temp Accom HRA")]
        TempAccomHRA,
        [Description("Travel Gen Fund")]
        TravelGenFund
    }
}
