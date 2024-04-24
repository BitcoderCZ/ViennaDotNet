using Serilog;
using System;
using CliUtils;
using CliUtils.Exceptions;

namespace ViennaDotNet.ObjectStore.Server
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

            Options options = new Options();
            options.addOption(Option.builder()
                .Option("dataDir")
                .LongOpt("dataDir")
                .HasArg()
                .ArgName("dir")
                .Desc("Directory where data is stored, defaults to ./data")
                .Build());
            options.addOption(Option.builder()
                .Option("port")
                .LongOpt("port")
                .HasArg()
                .ArgName("port")
                .Type(typeof(int))
                .Desc("Port to listen on, defaults to 5396")
                .Build());
            CommandLine commandLine;
            string dataDir;
            int port;
            try
            {
                commandLine = new DefaultParser().parse(options, args);
                dataDir = commandLine.hasOption("dataDir") ? commandLine.getOptionValue("dataDir")! : "./data";
                port = commandLine.hasOption("port") ? commandLine.getParsedOptionValue<int>("port") : 5396;
            }
            catch (ParseException exception)
            {
                Log.Fatal(exception.ToString());
                Environment.Exit(1);
                return;
            }

            NetworkServer server;
            try
            {
                server = new NetworkServer(new Server(new DataStore(new DirectoryInfo(dataDir))), port);
            }
            catch (Exception ex) when (
                ex is IOException
                || ex is DataStore.DataStoreException
            )
            {
                Log.Fatal(ex.ToString());
                Environment.Exit(1);
                return;
            }

            server.run();
        }
    }
}
