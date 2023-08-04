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
string connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? throw new ArgumentNullException(nameof(connectionString));

Console.WriteLine(connectionString);

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
