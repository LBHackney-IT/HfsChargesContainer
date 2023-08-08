using HfsChargesContainer;

Console.WriteLine("Application started!\nConfiguring the Start up!");

var entryPoint = new Startup()
    .ConfigureServices()
    .Build<ProcessEntryPoint>();

Console.WriteLine("Ready to Run!");
await entryPoint.Run();

Console.Error.WriteLine("Test Error Log Behaviour");

Console.WriteLine("Application finished!");
