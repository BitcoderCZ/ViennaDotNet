using Serilog;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using ViennaDotNet.Common;
using ViennaDotNet.Common.Utils;
using ViennaDotNet.Launcher.Programs;
using ViennaDotNet.Launcher.Utils;

namespace ViennaDotNet.Launcher;

internal sealed class LauncherWindow : Window
{
    private static readonly HttpClient httpClient = new();

    private static readonly string[] expectedStaticFiles = [
        "catalog/itemEfficiencyCategories.json",
        "catalog/itemJournalGroups.json",
        "catalog/items.json",
        "catalog/nfc.json",
        "catalog/recipes.json",
        "catalog/recipes.json",
        "server_jars/buildplate-connector-plugin-0.0.1-SNAPSHOT-jar-with-dependencies.jar",
        "server_jars/fountain-0.0.1-SNAPSHOT-jar-with-dependencies.jar",
        "tile_renderer/tagMap.json",
    ];

    private static readonly string[] expectedStaticDirectories = [
        "catalog",
        "encounters",
        "levels",
        "resourcepacks",
        "server_jars",
        "server_template_dir",
        "server_template_dir/mods",
        "tappables",
        "tile_renderer",
    ];

    private static readonly IEnumerable<string> programExes = [TileRenderer.ExeName, TappablesGenerator.ExeName, ApiServer.ExeName, BuildplateLauncher.ExeName, ObjectStoreServer.ExeName, EventBusServer.ExeName];

    private static Settings settings => Program.Settings;

    static LauncherWindow()
    {
        bool added = httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", $"BitcoderCZ/ViennaDotNet/{Assembly.GetExecutingAssembly().GetName().Version}");
        Debug.Assert(added);
    }

    public LauncherWindow()
    {
        Title = "ViennaDotNet Launcher";

        var startBtn = new Button()
        {
            X = Pos.Center(),
            Y = Pos.Absolute(1),
            Text = "_Start",
        };
        startBtn.Accepting += (s, e) =>
        {
            e.Handled = true;

            Start(settings);
        };

        var stopBtn = new Button()
        {
            X = Pos.Center(),
            Y = Pos.Bottom(startBtn) + 1,
            Text = "_Stop",
        };
        stopBtn.Accepting += (s, e) =>
        {
            e.Handled = true;

            Stop();
        };

        var optionsBtn = new Button()
        {
            X = Pos.Center(),
            Y = Pos.Bottom(stopBtn) + 1,
            Text = "_Options",
        };
        optionsBtn.Accepting += (s, e) =>
        {
            e.Handled = true;

            using var options = new OptionsWindow(settings)
            {
                X = Pos.Center(),
                Y = Pos.Center(),
                //Modal = true,
            };

            Application.Run(options);

            settings.Save(Program.SettingsFile);
        };

        var importBuildplateBtn = new Button()
        {
            X = Pos.Center(),
            Y = Pos.Bottom(optionsBtn) + 1,
            Text = "_Import buildplate",
        };
        importBuildplateBtn.Accepting += (s, e) =>
        {
            e.Handled = true;

            using var importBuildplate = new ImportBuildplateWindow(settings)
            {
                X = Pos.Center(),
                Y = Pos.Center(),
                //Modal = true,
            };

            Application.Run(importBuildplate);
        };

        var dataBtn = new Button()
        {
            X = Pos.Center(),
            Y = Pos.Bottom(importBuildplateBtn) + 1,
            Text = "_Modify data",
        };

        var exitBtn = new Button()
        {
            X = Pos.Center(),
            Y = Pos.Bottom(dataBtn) + 1,
            Text = "_Exit",
        };
        exitBtn.Accepting += (s, e) =>
        {
            Application.RequestStop();

            e.Handled = true;
        };

        Add(startBtn, stopBtn, optionsBtn, importBuildplateBtn, dataBtn, exitBtn);
    }

    private void Start(Settings settings)
        => UIUtils.RunWithLogs(this, async (logger, cancellationToken) =>
        {
            await Task.Yield();

            if (settings.SkipFileChecks is not true)
            {
                logger.Information("Validating files");
                await Check(settings, logger, cancellationToken);
            }
            else
            {
                logger.Warning("Skipped file validation, you can turn it back on in 'Configure/Skip file validation before starting'");
            }

            cancellationToken.ThrowIfCancellationRequested();

            EventBusServer.Run(settings, logger);
            cancellationToken.ThrowIfCancellationRequested();
            ObjectStoreServer.Run(settings, logger);
            cancellationToken.ThrowIfCancellationRequested();
            ApiServer.Run(settings, logger);
            cancellationToken.ThrowIfCancellationRequested();
            BuildplateLauncher.Run(settings, logger);
            cancellationToken.ThrowIfCancellationRequested();
            TappablesGenerator.Run(settings, logger);
            cancellationToken.ThrowIfCancellationRequested();
            TileRenderer.Run(settings, logger);

            logger.Information("Waiting for programs to start up");
            await Task.Delay(5000, cancellationToken); // wait a bit for them to start (and possible crash)

            bool error = false;
            foreach (string programExe in programExes)
            {
                if (!GetProgramProcess(programExe).Any())
                {
                    logger.Error($"It was detected that {programExe} crashed/exited, make sure all options are set correctly, look into logs/[program name]/logxxx for more info");
                    error = true;
                }
            }

            if (!error)
            {
                logger.Information("All programs (most likely) have started succesfully");
            }
        });

    private void Stop()
    {
        int selected = MessageBox.Query("Confirm", "Are you sure you want to stop all currently runnning server instances?", RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ["OK", "Cancel"] : ["Cancel", "OK"]);

        if (selected != (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 0 : 1))
        {
            return;
        }

        UIUtils.RunWithLogs(this, async (logger, cancellationToken) =>
        {
            await Task.Yield();

            foreach (string programName in programExes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await StopProgram(programName, logger, cancellationToken);
            }
        });
    }

    private static async Task StopProgram(string name, ILogger logger, CancellationToken cancellationToken)
    {
        logger.Information($"Stopping {name}");

        int stoppedCount = 0;
        foreach (var process in GetProgramProcess(name))
        {
            await process.StopGracefullyOrKillAsync(3000, cancellationToken);
            stoppedCount++;
        }

        logger.Information(stoppedCount switch
        {
            0 => $"No {name} processes found",
            1 => $"Stopped 1 {name} process",
            _ => $"Stopped {stoppedCount} {name} processes",
        });
    }

    private static IEnumerable<Process> GetProgramProcess(string name)
    {
        string exePath = Path.GetFullPath(name);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Debug.Assert(name.EndsWith(".exe", StringComparison.Ordinal));
            name = name[..^4];
        }

        foreach (var process in Process.GetProcessesByName(name))
        {
            if (process.MainModule is null || process.MainModule.FileName != exePath)
            {
                continue;
            }

            yield return process;
        }
    }

    private static async Task Check(Settings settings, ILogger logger, CancellationToken cancellationToken)
    {
        Debug.Assert(settings.SkipFileChecks is not true);

        bool error = false;
        if (!EventBusServer.Check(settings, logger) ||
            !ObjectStoreServer.Check(settings, logger) ||
            !ApiServer.Check(settings, logger) ||
            !BuildplateLauncher.Check(settings, logger) ||
            !TappablesGenerator.Check(settings, logger) ||
            !TileRenderer.Check(settings, logger))
        {
            error = true;
        }

        foreach (string dir in expectedStaticDirectories)
        {
            string fullDir = Path.GetFullPath(Path.Combine(Program.StaticDataDir, dir));

            if (!Directory.Exists(fullDir))
            {
                Directory.CreateDirectory(fullDir);
                logger.Warning($"Static data directory '{fullDir}' did not exist, created");
            }
        }

        foreach (string file in expectedStaticFiles)
        {
            string fullFile = Path.GetFullPath(Path.Combine(Program.StaticDataDir, file));

            if (!File.Exists(fullFile))
            {
                logger.Error($"Static data file '{fullFile}' does not exist");
                error = true;
            }
        }

        logger.Debug("All static files exist");

        string resourcePackPath = Path.GetFullPath(Path.Combine(Program.StaticDataDir, "resourcepacks", "vanilla.zip"));
        if (!File.Exists(resourcePackPath))
        {
            logger.Error($"Resourcepack file '{resourcePackPath}' does not exist");
            logger.Information("Download it from https://cdn.mceserv.net/availableresourcepack/resourcepacks/dba38e59-091a-4826-b76a-a08d7de5a9e2-1301b0c257a311678123b9e7325d0d6c61db3c35 (using internet archive)");
            logger.Information($"Rename it to vanilla.zip and move it to: {Path.GetFullPath(Path.Combine(Program.StaticDataDir, "resourcepacks"))}");

            error = true;
        }

        if (!Directory.EnumerateFiles(Path.Combine(Program.StaticDataDir, "server_template_dir", "mods")).Any(path => Path.GetFileName(path).StartsWith("fabric-api", StringComparison.OrdinalIgnoreCase) && path.EndsWith(".jar", StringComparison.OrdinalIgnoreCase)))
        {
            logger.Warning("Fabric api mod not found, downloading");

            var response = await httpClient.GetAsync("https://cdn.modrinth.com/data/P7dR8mSH/versions/9p2sguD7/fabric-api-0.96.4%2B1.20.4.jar", cancellationToken);
            using (var fs = File.OpenWrite(Path.Combine(Program.StaticDataDir, "server_template_dir", "mods", "fabric-api-0.96.4+1.20.4.jar")))
            {
                await response.Content.CopyToAsync(fs, cancellationToken);
            }

            logger.Information("Downloaded fabric api");
        }

        if (!File.Exists(Path.Combine(Program.StaticDataDir, "server_template_dir", BuildplateLauncher.ServerJarName)))
        {
            logger.Warning("Fabric server not found, downloading");

            var response = await httpClient.GetAsync("https://meta.fabricmc.net/v2/versions/loader/1.20.4/0.15.7/1.0.3/server/jar", cancellationToken);
            using (var fs = File.OpenWrite(Path.Combine(Program.StaticDataDir, "server_template_dir", BuildplateLauncher.ServerJarName)))
            {
                await response.Content.CopyToAsync(fs, cancellationToken);
            }

            logger.Information("Downloaded fabric server");
        }

        string eulaPath = Path.GetFullPath(Path.Combine(Program.StaticDataDir, "server_template_dir", "eula.txt"));
        if (!File.Exists(eulaPath))
        {
            logger.Information("Detected that server was not setup, running");

            string javaExe = JavaLocator.locateJava(logger);

            bool useShellExecute = false;

            var serverProcess = new ConsoleProcess(javaExe, useShellExecute, !useShellExecute);

            if (!useShellExecute)
            {
                serverProcess.StandartTextReceived += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        logger.Debug($"[server] {e.Data}");
                    }
                };
                serverProcess.ErrorTextReceived += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        logger.Error($"[server] {e.Data}");
                    }
                };
            }

            serverProcess.ExecuteAsync(Path.GetFullPath(Path.Combine(Program.StaticDataDir, "server_template_dir")), ["-jar", BuildplateLauncher.ServerJarName, "-nogui"]);
            logger.Information("Server process started, waiting for exit");
            await serverProcess.Process.WaitForExitAsync(cancellationToken);

            int exitCode = serverProcess.Process.ExitCode;
            logger.Information($"Server process exited with exit code {exitCode}");
            if (exitCode != 0)
            {
                error = true;
            }
        }

        if (File.Exists(eulaPath) && !(await File.ReadAllTextAsync(eulaPath, cancellationToken)).Contains("eula=true", StringComparison.OrdinalIgnoreCase))
        {
            logger.Information($"Server eula not accepted, open '{eulaPath}' and set 'eula=true'");
            logger.Information("Waiting for you to make the change");
            while (!(await File.ReadAllTextAsync(eulaPath, cancellationToken)).Contains("eula=true", StringComparison.OrdinalIgnoreCase))
            {
                await Task.Delay(1000, cancellationToken);
            }

            logger.Information("Running server to download/generate rest of the files, close it after it starts up");

            string javaExe = JavaLocator.locateJava(logger);

            bool useShellExecute = true;

            var serverProcess = new ConsoleProcess(javaExe, useShellExecute, !useShellExecute);

            if (!useShellExecute)
            {
                serverProcess.StandartTextReceived += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        logger.Debug($"[server] {e.Data}");
                    }
                };
                serverProcess.ErrorTextReceived += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        logger.Error($"[server] {e.Data}");
                    }
                };
            }

            serverProcess.ExecuteAsync(Path.GetFullPath(Path.Combine(Program.StaticDataDir, "server_template_dir")), ["-jar", BuildplateLauncher.ServerJarName, "-nogui"]);
            logger.Information("Server process started, waiting for exit");
            await serverProcess.Process.WaitForExitAsync(cancellationToken);

            int exitCode = serverProcess.Process.ExitCode;
            logger.Information($"Server process exited with exit code {exitCode}");
            if (exitCode != 0)
            {
                error = true;
            }
        }

        if (error)
        {
            throw new Exception("File validation failed.");
        }
    }
}
