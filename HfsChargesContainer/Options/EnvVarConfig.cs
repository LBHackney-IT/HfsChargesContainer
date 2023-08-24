namespace HfsChargesContainer.Options
{
    /// <summary>
    /// These options define which environment variables get pulled down from SSM during runtime.
    /// 
    /// If you want to add/remove an SSM key from being pulled down, then add/remove it from this
    /// dictionary.
    /// 
    /// Similarly, if you want to make some application variable to look at a different SSM key, then
    /// simply change the name of SSM key name next to app variable key name.
    /// </summary>
    public class EnvVarConfig
    {
        public Dictionary<string, string> EnvVarSSMToAppNameLookup { get; }

        public EnvVarConfig(string environment)
        {
            EnvVarSSMToAppNameLookup = new()
            {
                { $"/housing-finance/{environment}/charges-batch-years", "CHARGES_BATCH_YEARS" },
                { $"/housing-finance/{environment}/bulk-insert-batch-size", "BATCH_SIZE" },
                { $"/housing-finance/{environment}/google-application-credentials-json", "GOOGLE_API_KEY" },
                { $"/housing-finance/{environment}/db-host", "DB_HOST" },
                { $"/housing-finance/{environment}/db-database", "DB_NAME" },
                { $"/housing-finance/{environment}/db-username", "DB_USER" },
                { $"/housing-finance/{environment}/db-password", "DB_PASSWORD" },
            };
        }
    }
}
