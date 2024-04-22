using Serilog;
using ViennaDotNet.EventBus.Client;

namespace ViennaDotNet.TappablesGenerator
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            var log = new LoggerConfiguration()
               .WriteTo.Console()
               .WriteTo.File("logs/debug.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, fileSizeLimitBytes: 8338607, outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
               .MinimumLevel.Debug()
               .CreateLogger();

            Log.Logger = log;

            string eventBusConnectionString = "localhost:5532";

            Log.Information("Connecting to event bus");
            EventBusClient eventBusClient;
            try
            {
                eventBusClient = EventBusClient.create(eventBusConnectionString);
            }
            catch (EventBusClientException exception)
            {
                Log.Fatal($"Could not connect to event bus: {exception}");
                Environment.Exit(1);
                return;
            }
            Log.Information("Connected to event bus");

            Generator generator = new Generator();
            ActiveTiles activeTiles = new ActiveTiles(eventBusClient);
            Spawner spawner = new Spawner(eventBusClient, activeTiles, generator);
            spawner.run();
        }
    }
}
