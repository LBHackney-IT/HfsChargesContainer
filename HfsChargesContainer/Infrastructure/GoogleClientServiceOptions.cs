using System.Collections.Generic;

namespace HfsChargesContainer.Infrastructure
{
    public class GoogleClientServiceOptions
    {
        public string CredentialsPath { get; set; }

        /// <summary>
        /// Gets or sets the application's permission scopes.
        /// </summary>
        public IList<string> Scopes { get; set; }
        public string RedirectUri { get; set; }
        public string ApplicationName { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
