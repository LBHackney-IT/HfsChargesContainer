using HfsChargesContainer;
using HfsChargesContainer.Gateways;
using HfsChargesContainer.Gateways.Interfaces;
using HfsChargesContainer.Infrastructure;
using HfsChargesContainer.UseCases;
using HfsChargesContainer.UseCases.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Application started!");

Console.WriteLine("Configuring the Start up!");
string dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? throw new ArgumentNullException(nameof(dbHost));
string dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? throw new ArgumentNullException(nameof(dbName));
string dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? throw new ArgumentNullException(nameof(dbUser));
string dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? throw new ArgumentNullException(nameof(dbPassword));

string connectionString = $"Data Source={dbHost},1433;Initial Catalog={dbName};Integrated Security=False;User Id={dbUser};Password=**redacted**;Encrypt=False;TrustServerCertificate=False;MultipleActiveResultSets=True;";

var serviceProvider = new ServiceCollection()
    .AddDbContext<DatabaseContext>(opt =>
        opt.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.CommandTimeout(360);
        })
    )
    .AddScoped<IHousingFinanceGateway, HousingFinanceGateway>()
    .AddScoped<IUseCase1, UseCase1>()
    .AddScoped<ProcessEntryPoint>()
    .BuildServiceProvider();

var entryPoint = serviceProvider.GetRequiredService<ProcessEntryPoint>();

Console.WriteLine("Ready to Run!");
entryPoint.Run();

Console.WriteLine("Application finished!");
