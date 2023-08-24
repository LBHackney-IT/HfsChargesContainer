using HfsChargesContainer.Domain;
using HfsChargesContainer.Infrastructure.Models;

namespace HfsChargesContainer.Factories
{
    public static class GoogleFileSettingFactory
    {
        public static GoogleFileSettingDomain ToDomain(this GoogleFileSetting googleFileSetting)
        {
            return new GoogleFileSettingDomain
            {
                Id = googleFileSetting.Id,
                Label = googleFileSetting.Label,
                FileYear = googleFileSetting.FileYear,
                FileType = googleFileSetting.FileType,
                GoogleIdentifier = googleFileSetting.GoogleIdentifier,
                StartDate = googleFileSetting.StartDate,
                EndDate = googleFileSetting.EndDate
            };
        }

        public static List<GoogleFileSettingDomain> ToDomain(
            this ICollection<GoogleFileSetting> googleFileSetting)
        {
            return googleFileSetting?.Select(uO => uO.ToDomain()).ToList();
        }
    }
}
