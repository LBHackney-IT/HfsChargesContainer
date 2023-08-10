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
            ConfigureDatabase(services);
            ConfigureGateways(services);
            ConfigureUseCases(services);
            ConfigureEntry(services);
            ConfigureGoogleClient(services);

            return this;
        }

        #region Environmnent and options
        public string GetConnectionString()
        {
            string dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? throw new ArgumentNullException(nameof(dbHost));
            string dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? throw new ArgumentNullException(nameof(dbName));
            string dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? throw new ArgumentNullException(nameof(dbUser));
            string dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? throw new ArgumentNullException(nameof(dbPassword));

            return $"Data Source={dbHost},1433;Initial Catalog={dbName};Integrated Security=False;User Id={dbUser};Password={dbPassword};Encrypt=False;TrustServerCertificate=False;MultipleActiveResultSets=True;";
        }

        public string GetGCPJsonCredentials()
            => Environment.GetEnvironmentVariable("GOOGLE_API_KEY")
            ?? throw new ArgumentNullException("Google Cloud credentials are missing.");

        public string GetChargesBatchYears()
            => Environment.GetEnvironmentVariable("CHARGES_BATCH_YEARS")
            ?? throw new ArgumentNullException("Charges Batch Years are missing.");

        public string GetBatchSize()
            => Environment.GetEnvironmentVariable("BATCH_SIZE")
            ?? throw new ArgumentNullException("Batch Size is missing.");

        public void ConfigureOptions(IServiceCollection services)
        {
            var chargesBatchYears = GetChargesBatchYears();
            var chargesBulkInsertBatchSize = Convert.ToInt32(GetBatchSize());

            services.AddScoped(_ => new ChargesBatchYearsOptions(chargesBatchYears));
            services.AddScoped(_ => new ChargesGWOptions(chargesBulkInsertBatchSize));
        }
        #endregion
        #region External Storage
        public void ConfigureDatabase(IServiceCollection services)
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

        public void ConfigureGateways(IServiceCollection services)
        {
            services.AddScoped<IChargesGateway, ChargesGateway>();
            services.AddScoped<IBatchLogGateway, BatchLogGateway>();
            services.AddScoped<IGoogleClientService, GoogleClientService>();
            services.AddScoped<IBatchLogErrorGateway, BatchLogErrorGateway>();
            services.AddScoped<IHousingFinanceGateway, HousingFinanceGateway>();
            services.AddScoped<IChargesBatchYearsGateway, ChargesBatchYearsGateway>();
            services.AddScoped<IChargesBatchYearsGateway, ChargesBatchYearsGateway>();
            services.AddScoped<IGoogleFileSettingGateway, GoogleFileSettingGateway>();
        }

        public void ConfigureUseCases(IServiceCollection services)
        {
            services.AddScoped<IUseCase1, UseCase1>();
            services.AddScoped<ILoadChargesUseCase, LoadChargesUseCase>();
            services.AddScoped<ICheckChargesBatchYearsUseCase, CheckChargesBatchYearsUseCase>();
        }

        public void ConfigureEntry(IServiceCollection services)
        {
            services.AddScoped<ProcessEntryPoint>();
        }

        public IEntry Build<TEntry>() where TEntry : IEntry
        {
            var serviceProvider = this.ServiceCollection.BuildServiceProvider();
            return serviceProvider.GetRequiredService<TEntry>();
        }
    }
}
