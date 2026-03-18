using Serilog;
using System.Collections.ObjectModel;
using Terminal.Gui.App;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using ViennaDotNet.Common.Utils;

namespace ViennaDotNet.Launcher.Utils;

internal static class UIUtils
{
    public static Task RunWithLogsAsync(Window window, bool closeOnOk, Func<ILogger, CancellationToken, Task> action)
    {
        var tokenSource = new CancellationTokenSource();

        var view = new FrameView()
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };

        var logs = new ObservableCollection<string>();
        var list = new ListView()
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };
        list.VerticalScrollBar.AutoShow = true;
        list.VerticalScrollBar.Enabled = true;
        list.HorizontalScrollBar.AutoShow = true;
        list.HorizontalScrollBar.Enabled = true;
        list.SetSource(logs);

        var btn = new Button()
        {
            Text = "_Cancel",
            X = Pos.Center(),
            Y = Pos.AnchorEnd(),
        };
        btn.Accepting += (s, e) =>
        {
            e.Handled = true;

            tokenSource.Cancel();

            window.Remove(view);
        };

        view.Add(list, btn);
        window.Add(view);

        var logger = Program.LoggerConfiguration
            .WriteTo.Collection(logs)
            .CreateLogger();

        return RunAction(action, logger, tokenSource.Token)
            .ContinueWith(lastTask =>
            {
                bool anyExceptions = false;

                if (lastTask.Exception is { } aggEx)
                {
                    foreach (var ex in aggEx.InnerExceptions)
                    {
                        anyExceptions = true;
                        logger.Error($"Exception: {ex}");
                    }
                }

                btn.Text = "_OK";

                if (!anyExceptions && closeOnOk)
                {
                    tokenSource.Cancel();

                    window.Remove(view);
                }
            });

        static async Task RunAction(Func<ILogger, CancellationToken, Task> action, ILogger logger, CancellationToken cancellationToken)
        {
            await Task.Yield();
            await action(logger, cancellationToken);
        }
    }

    public static void Load(Window window, Func<CancellationToken, Task> task, string loadingText = "Loading...")
    {
        LoadAsync(window, task, loadingText)
            .Forget();

        static async Task LoadAsync(Window window, Func<CancellationToken, Task> task, string loadingText)
        {
            await Task.Yield();

            var loadingLabel = new Label()
            {
                Text = loadingText,
                X = Pos.Center(),
                Y = Pos.Center(),
            };

            window.Add(loadingLabel);

            var tokenSource = new CancellationTokenSource();

            window.KeyDown += KeyDown;

            try
            {
                await task(tokenSource.Token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("Error", $"An error occurred: {ex.GetInnerMostMessage()}", "OK");
                Application.RequestStop(window);
            }
            finally
            {
                window.Remove(loadingLabel);
                window.KeyDown -= KeyDown;
            }

            void KeyDown(object? sender, Key e)
            {
                if (e.KeyCode == Application.QuitKey.KeyCode)
                {
                    tokenSource.Cancel();
                    window.KeyDown -= KeyDown;
                }
            }
        }
    }
}
