using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using HfsChargesContainer.Gateways;
using HfsChargesContainer.Gateways.Interfaces;
using HfsChargesContainer.Infrastructure;
using HfsChargesContainer.Options;
using HfsChargesContainer.UseCases;
using HfsChargesContainer.UseCases.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HfsChargesContainer
{
    public class Startup
    {
        private IServiceCollection ServiceCollection { get; set; }
        public Startup(IServiceCollection serviceCollection = null)
        {
            ServiceCollection = serviceCollection ?? new ServiceCollection();
        }

        public Startup ConfigureServices()
        {
            var services = this.ServiceCollection;
            ConfigureOptions(services);
            ConfigureDatabaseContext(services);
            ConfigureGateways(services);
            ConfigureUseCases(services);
            ConfigureEntry(services);
            ConfigureGoogleClient(services);

            return this;
        }

        public IEntry Build<TEntry>() where TEntry : IEntry
        {
            var serviceProvider = this.ServiceCollection.BuildServiceProvider();
            return serviceProvider.GetRequiredService<TEntry>();
        }

        public void ConfigureEntry(IServiceCollection services)
        {
            services.AddScoped<ProcessEntryPoint>();
        }

        public void ConfigureUseCases(IServiceCollection services)
        {
            services.AddScoped<ILoadChargesUseCase, LoadChargesUseCase>();
            services.AddScoped<ILoadChargesHistoryUseCase, LoadChargesHistoryUseCase>();
            services.AddScoped<ILoadChargesTransactionsUseCase, LoadChargesTransactionsUseCase>();
            services.AddScoped<ICheckChargesBatchYearsUseCase, CheckChargesBatchYearsUseCase>();
        }

        public void ConfigureGateways(IServiceCollection services)
        {
            services.AddScoped<IChargesGateway, ChargesGateway>();
            services.AddScoped<IBatchLogGateway, BatchLogGateway>();
            services.AddScoped<ITransactionGateway, TransactionGateway>();
            services.AddScoped<IGoogleClientService, GoogleClientService>();
            services.AddScoped<IBatchLogErrorGateway, BatchLogErrorGateway>();
            services.AddScoped<IChargesBatchYearsGateway, ChargesBatchYearsGateway>();
            services.AddScoped<IChargesBatchYearsGateway, ChargesBatchYearsGateway>();
            services.AddScoped<IGoogleFileSettingGateway, GoogleFileSettingGateway>();
        }

        #region External Storage Interfaces
        public void ConfigureDatabaseContext(IServiceCollection services)
        {
            var hfsDbConnectionString = GetConnectionString();

            services.AddDbContext<DatabaseContext>(opt =>
                opt.UseSqlServer(hfsDbConnectionString, sqlOptions =>
                {
                    sqlOptions.CommandTimeout(360);
                })
            );
        }

        public void ConfigureGoogleClient(IServiceCollection services)
        {
            var gcsOptions = new GoogleClientServiceOptions
            {
                ApplicationName = "HFS Charges Ingest Container",
                Scopes = new List<string>
                {
                    SheetsService.Scope.SpreadsheetsReadonly
                }
            };

            string jsonKey = GetGCPJsonCredentials();
            var credential = GoogleCredential.FromJson(jsonKey).CreateScoped(gcsOptions.Scopes);

            var baseClientService = new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = gcsOptions.ApplicationName
            };

            services.AddScoped(sp => new SheetsService(baseClientService));
            services.AddScoped<IGoogleClientService, GoogleClientService>();
        }
        #endregion

        #region Environmnent and options
        public void ConfigureOptions(IServiceCollection services)
        {
            var chargesBatchYears = GetChargesBatchYears();
            var chargesBulkInsertBatchSize = Convert.ToInt32(GetBatchSize());

            services.AddScoped(_ => new ChargesBatchYearsOptions(chargesBatchYears));
            services.AddScoped(_ => new ChargesGWOptions(chargesBulkInsertBatchSize));
        }

        public string GetEnvVarOrThrow(string envVarName, string errorMsgIfMissing)
            => Environment.GetEnvironmentVariable(envVarName)
            ?? throw new ArgumentNullException(errorMsgIfMissing);

        public string GetConnectionString()
        {
            string dbHost = GetEnvVarOrThrow("DB_HOST", nameof(dbHost));
            string dbName = GetEnvVarOrThrow("DB_NAME", nameof(dbName));
            string dbUser = GetEnvVarOrThrow("DB_USER", nameof(dbUser));
            string dbPassword = GetEnvVarOrThrow("DB_PASSWORD", nameof(dbPassword));

            return $"Data Source={dbHost},1433;Initial Catalog={dbName};Integrated Security=False;User Id={dbUser};Password={dbPassword};Encrypt=False;TrustServerCertificate=False;MultipleActiveResultSets=True;";
        }

        public string GetGCPJsonCredentials() => GetEnvVarOrThrow("GOOGLE_API_KEY", "Google Cloud credentials are missing.");
        public string GetChargesBatchYears() => GetEnvVarOrThrow("CHARGES_BATCH_YEARS", "Charges Batch Years are missing.");
        public string GetBatchSize() => GetEnvVarOrThrow("BATCH_SIZE", "Batch Size is missing.");
        #endregion
    }
}
