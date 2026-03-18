using System.Diagnostics;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using ViennaDotNet.Launcher.Programs;
using ViennaDotNet.Launcher.Utils;

namespace ViennaDotNet.Launcher.Windows;

internal sealed class ImportBuildplateWindow : Window
{
    public ImportBuildplateWindow(Settings settings)
    {
        const int ChoiceButtonsGroup = 0;

        Title = "Import buidplate";

        var idLabel = new Label()
        {
            Text = "Player ID:",
        };

        var idInput = new TextField()
        {
            X = Pos.Right(idLabel) + 1,
            Y = Pos.Y(idLabel),
            Width = Dim.Fill(),
        };

        var worldFileLabel = new Label()
        {
            Text = "Buildplate file:",
            X = Pos.X(idLabel),
            Y = Pos.Bottom(idLabel) + 1,
        };
        var worldFileInput = new Button
        {
            Text = "Not selected",
            X = Pos.Right(worldFileLabel) + 1,
            Y = Pos.Y(worldFileLabel),
        };
        worldFileInput.Accepting += (s, e) =>
        {
            var dialog = new OpenDialog()
            {
                AllowedTypes = [new AllowedType("Vienna", ".zip"), new AllowedType("Project Earth", ".json")],
                AllowsMultipleSelection = false,
                MustExist = true,
                ShadowStyle = ShadowStyle.Transparent,
                OpenMode = OpenMode.File,
            };

            Application.Run(dialog);

            if (dialog.FilePaths.Count == 0)
            {
                worldFileInput.Text = "Not selected";
            }
            else
            {
                worldFileInput.Text = dialog.FilePaths[0];
            }

            e.Handled = true;
        };

        var cancelBtn = new Button()
        {
            Text = "Cancel",
            X = Pos.Align(Alignment.Center, AlignmentModes.AddSpaceBetweenItems, ChoiceButtonsGroup),
            Y = Pos.AnchorEnd(),
        };
        cancelBtn.Accepting += (s, e) =>
        {
            e.Handled = true;

            Application.RequestStop();
        };

        var importBtn = new Button()
        {
            Text = "Import",
            X = Pos.Align(Alignment.Center, AlignmentModes.AddSpaceBetweenItems, ChoiceButtonsGroup),
            Y = Pos.AnchorEnd(),
        };
        importBtn.Accepting += (s, e) =>
        {
            e.Handled = true;

            string playerId = idInput.Text.ToLowerInvariant();
            string? file = worldFileInput.Text is "Not selected" ? null : worldFileInput.Text;

            if (string.IsNullOrEmpty(file))
            {
                MessageBox.ErrorQuery("Error", "Buildplate file not selected", "OK");
            }
            else if (!File.Exists(file) || Path.GetExtension(file) is not (".zip" or ".json"))
            {
                MessageBox.ErrorQuery("Error", "Invalid buildplate file", "OK");
            }
            else if (string.IsNullOrWhiteSpace(playerId) || playerId.Length == 0)
            {
                MessageBox.ErrorQuery("Error", "Invalid player id", "OK");
            }
            else
            {
                Import(settings, playerId, file);
            }
        };

        Add(idLabel, idInput, worldFileLabel, worldFileInput, cancelBtn, importBtn);
    }

    private void Import(Settings settings, string playerId, string filePath)
        => UIUtils.RunWithLogsAsync(this, false, async (logger, cancellationToken) =>
        {
            await FileChecker.Check(settings, true, logger, cancellationToken);

            bool generatePreview = settings.GeneratePreviewOnImport is not false;

            logger.Information("Starting or reusing processed");
            var eventBus = ProcessUtils.StartIfNotRunning(EventBusServer.ExeName, () => EventBusServer.Run(settings, logger));
            var objectStore = ProcessUtils.StartIfNotRunning(ObjectStoreServer.ExeName, () => ObjectStoreServer.Run(settings, logger));
            // generates the preview
            Process? buildplateLauncher;
            if (generatePreview)
            {
                buildplateLauncher = ProcessUtils.StartIfNotRunning(BuildplateLauncher.ExeName, () => BuildplateLauncher.Run(settings, logger));
            }
            else
            {
                buildplateLauncher = null;
                logger.Warning("Preview will not be generated, preview generation on import can be turned on in options");
            }

            logger.Information("Waiting for programs to start up");
            await Task.Delay(5000, cancellationToken); // wait a bit for them to start (and possible crash)

            if (eventBus is null || eventBus.HasExited)
            {
                logger.Error($"{EventBusServer.DispName} failed to start or crashed/exited");
                return;
            }

            if (objectStore is null || objectStore.HasExited)
            {
                logger.Error($"{ObjectStoreServer.DispName} failed to start or crashed/exited");
                return;
            }

            if (generatePreview && (buildplateLauncher is null || buildplateLauncher.HasExited))
            {
                logger.Error($"{BuildplateLauncher.DispName} failed to start or crashed/exited, preview generation on import can be turned off in options");
                return;
            }

            var importer = BuildplateImporter.Run(settings, playerId, filePath, true, logger);

            logger.Information($"Waiting for {BuildplateImporter.DispName} process to exit");

            await importer.WaitForExitAsync(cancellationToken);

            logger.Information($"{BuildplateImporter.DispName} process exited with exit code {importer.ExitCode}");
        });
}
