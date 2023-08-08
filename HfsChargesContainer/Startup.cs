using System;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using HfsChargesContainer.Gateways;
using HfsChargesContainer.Gateways.Interfaces;
using HfsChargesContainer.Infrastructure;
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
            ConfigureStorageInterfaces(services);
            ConfigureGateways(services);
            ConfigureUseCases(services);
            ConfigureEntry(services);
            ConfigureGoogleClient(services);

            return this;
        }

        public string GetConnectionString()
        {
            string dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? throw new ArgumentNullException(nameof(dbHost));
            string dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? throw new ArgumentNullException(nameof(dbName));
            string dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? throw new ArgumentNullException(nameof(dbUser));
            string dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? throw new ArgumentNullException(nameof(dbPassword));

            Console.WriteLine("Envs");
            Console.WriteLine($"DB_NAME: ***{dbName}***");
            Console.WriteLine($"GCP_JSON: ***{Environment.GetEnvironmentVariable("GOOGLE_API_KEY")?.Substring(0,10)}***");

            return $"Data Source={dbHost},1433;Initial Catalog={dbName};Integrated Security=False;User Id={dbUser};Password={dbPassword};Encrypt=False;TrustServerCertificate=False;MultipleActiveResultSets=True;";
        }

        public string GetGCPJsonCredentials()
        {
            string gcpCreds = Environment.GetEnvironmentVariable("GOOGLE_API_KEY") ?? throw new ArgumentNullException("Google Cloud credentials are missing.");
            return gcpCreds;
        }

        public void ConfigureStorageInterfaces(IServiceCollection services)
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

        public void ConfigureGateways(IServiceCollection services)
        {
            services.AddScoped<IHousingFinanceGateway, HousingFinanceGateway>();
        }

        public void ConfigureUseCases(IServiceCollection services)
        {
            services.AddScoped<IUseCase1, UseCase1>();
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
