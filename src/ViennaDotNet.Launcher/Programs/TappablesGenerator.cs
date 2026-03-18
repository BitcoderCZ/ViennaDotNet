using Serilog;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ViennaDotNet.Launcher.Programs;

internal static class TappablesGenerator
{
    public static readonly string ExeName = "TappablesGenerator" + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "");
    public const string DispName = "Tappable generator";

    public static bool Check(Settings settings, ILogger logger)
    {
        string exePath = Path.GetFullPath(Path.Combine(Program.ProgramsDir, ExeName));
        if (!File.Exists(exePath))
        {
            logger.Error($"{DispName} exe doesn't exits: {exePath}");
            return false;
        }

        return true;
    }

    public static void Run(Settings settings, ILogger logger)
    {
        logger.Information($"Running {DispName}");
        Process.Start(new ProcessStartInfo(Path.GetFullPath(Path.Combine(Program.ProgramsDir, ExeName)),
        [
            $"--eventbus=localhost:{settings.EventBusPort}"
        ])
        {
            WorkingDirectory = Path.GetFullPath(Program.ProgramsDir),
            CreateNoWindow = false,
            UseShellExecute = true
        });
    }
}
