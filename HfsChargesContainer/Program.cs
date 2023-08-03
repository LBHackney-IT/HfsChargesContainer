using HfsChargesContainer;
using HfsChargesContainer.UseCases;
using HfsChargesContainer.UseCases.Interfaces;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Application started!");

var serviceProvider = new ServiceCollection()
    .AddScoped<IUseCase1, UseCase1>()
    .AddScoped<ProcessEntryPoint>()
    .BuildServiceProvider();

var entryPoint = serviceProvider.GetRequiredService<ProcessEntryPoint>();
entryPoint.Run();

Console.WriteLine("Application finished!");
