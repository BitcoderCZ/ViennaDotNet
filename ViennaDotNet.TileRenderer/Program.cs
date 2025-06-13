using CommandLine;
using Npgsql;
using Serilog;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ViennaDotNet.DB;
using ViennaDotNet.EventBus.Client;
using ViennaDotNet.ObjectStore.Client;
using ViennaDotNet.StaticData;
using ViennaDotNet.TileRenderer;

internal static class Program
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private sealed class Options
    {
        [Option("db", Default = "./earth.db", Required = false, HelpText = "Database connection string")]
        public string DatabaseConnectionString { get; set; }

        [Option("tileDB", Required = true, HelpText = "Connection string to a postgresql database with tile data, for example 'Host=myserver;Username=mylogin;Password=mypass;Database=mydatabase'")]
        public string TileDatabaseConnectionString { get; set; }

        [Option("eventbus", Default = "localhost:5532", Required = false, HelpText = "Event bus address")]
        public string EventBusConnectionString { get; set; }

        [Option("objectstore", Default = "localhost:5396", Required = false, HelpText = "Object storage address")]
        public string ObjectStoreConnectionString { get; set; }

        [Option("dir", Default = "./data", Required = false, HelpText = "Static data path")]
        public string StaticDataPath { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private static async Task<int> Main(string[] args)
    {
        var log = new LoggerConfiguration()
           .WriteTo.Console()
           .WriteTo.File("logs/debug.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, fileSizeLimitBytes: 8338607, outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
           .MinimumLevel.Debug()
           .CreateLogger();

        Log.Logger = log;

        if (!Debugger.IsAttached)
        {
            AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
            {
                Log.Fatal($"Unhandeled exception: {e.ExceptionObject}");
                Log.CloseAndFlush();
                Environment.Exit(1);
            };
        }

        ParserResult<Options> res = Parser.Default.ParseArguments<Options>(args);

        Options options;
        if (res is Parsed<Options> parsed)
        {
            options = parsed.Value;
        }
        else
        {
            return res is NotParsed<Options> notParsed
                ? res.Errors.Any(error => error is HelpRequestedError) 
                    ? 0 
                    : res.Errors.Any(error => error is VersionRequestedError) 
                    ? 0 
                    : 1
                : 1;
        }


        EarthDB earthDB;
        Log.Information("Connecting to main database");
        try
        {
            earthDB = EarthDB.Open(options.DatabaseConnectionString);
        }
        catch (EarthDB.DatabaseException ex)
        {
            Log.Fatal($"Could not connect to main database: {ex}");
            Log.CloseAndFlush();
            return 1;
        }

        NpgsqlDataSource tileDB;
        Log.Information("Connecting to tile database");
        try
        {
            tileDB = NpgsqlDataSource.Create(options.TileDatabaseConnectionString);
        }
        catch (Exception ex)
        {
            Log.Fatal($"Could not connect to tile database: {ex}");
            Log.CloseAndFlush();
            return 1;
        }

        Log.Information("Connecting to event bus");
        EventBusClient eventBusClient;
        try
        {
            eventBusClient = EventBusClient.create(options.EventBusConnectionString);
        }
        catch (EventBusClientException ex)
        {
            tileDB.Dispose();

            Log.Fatal($"Could not connect to event bus: {ex}");
            Log.CloseAndFlush();
            return 1;
        }

        Log.Information("Connected to event bus");

        ObjectStoreClient objectStore;
        Log.Information("Connecting to object storage");
        try
        {
            objectStore = ObjectStoreClient.create(options.ObjectStoreConnectionString);
        }
        catch (ObjectStoreClientException ex)
        {
            Log.Fatal($"Could not connect to object storage: {ex}");
            Log.CloseAndFlush();
            return 1;
        }

        Log.Information("Connected to object storage");

        Log.Information("Loading static data");
        StaticData staticData;
        try
        {
            staticData = new StaticData(options.StaticDataPath);
        }
        catch (StaticDataException staticDataException)
        {
            tileDB.Dispose();

            Log.Fatal($"Failed to load static data: {staticDataException}");
            Log.CloseAndFlush();
            return 1;
        }

        Log.Information("Loaded static data");

        using (EventBusTileRenderer renderer = new EventBusTileRenderer(earthDB, tileDB, eventBusClient, objectStore, staticData))
        {
            renderer.Run();
        }

        return 0;
    }
}