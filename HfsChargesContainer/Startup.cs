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
        public Startup()
        {}

        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureStorageInterfaces(services);
            ConfigureGateways(services);
            ConfigureUseCases(services);
            ConfigureEntry(services);
        }

        public string GetConnectionString()
        {
            string dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? throw new ArgumentNullException(nameof(dbHost));
            string dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? throw new ArgumentNullException(nameof(dbName));
            string dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? throw new ArgumentNullException(nameof(dbUser));
            string dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? throw new ArgumentNullException(nameof(dbPassword));

            return $"Data Source={dbHost},1433;Initial Catalog={dbName};Integrated Security=False;User Id={dbUser};Password={dbPassword};Encrypt=False;TrustServerCertificate=False;MultipleActiveResultSets=True;";
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

        public IEntry Build<TEntry>(IServiceCollection services) where TEntry : IEntry
        {
            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetRequiredService<TEntry>();
        }
    }
}
