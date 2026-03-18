using Serilog;
using System.Diagnostics;
using System.Globalization;
using Terminal.Gui.App;
using ViennaDotNet.Launcher.Windows;

namespace ViennaDotNet.Launcher;

internal static class Program
{
    public const string SettingsFile = "config.json";
    public const string ProgramsDir = "./"; // same as launcher
    public const string StaticDataDir = "staticdata";

    public static LoggerConfiguration LoggerConfiguration => new LoggerConfiguration()
            .WriteTo.Conditional(e => LogToConsole, wt => wt.Console())
            .WriteTo.File("logs/launcher/log.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, fileSizeLimitBytes: 8338607, outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .MinimumLevel.Debug();

    public static Settings Settings = Settings.Default;

    public static bool LogToConsole = true;

    static async Task Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

        var log = LoggerConfiguration.CreateLogger();

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

        await AutoUpdater.CheckAndUpdate();

        Settings = await Settings.LoadAsync(SettingsFile);

        LogToConsole = false;

        Application.Run<LauncherWindow>().Dispose();

        Application.Shutdown();
    }
}
