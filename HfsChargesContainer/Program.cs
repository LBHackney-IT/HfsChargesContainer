using HfsChargesContainer;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Application started!");

Console.WriteLine("Configuring the Start up!");
var startup = new Startup();
var serviceCollection = new ServiceCollection();
startup.ConfigureServices(serviceCollection);
var entryPoint = startup.Build<ProcessEntryPoint>(serviceCollection);

Console.WriteLine("Ready to Run!");
entryPoint.Run();

Console.WriteLine("Application finished!");
