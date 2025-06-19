using Serilog;
using System.Collections.ObjectModel;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using ViennaDotNet.Common.Utils;

namespace ViennaDotNet.Launcher.Utils;

internal static class UIUtils
{
    public static void RunWithLogs(Window window, Func<ILogger, CancellationToken, Task> action)
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

        action(logger, tokenSource.Token)
            .ContinueWith(lastTask =>
            {
                if (lastTask.Exception is { } aggEx)
                {
                    foreach (var ex in aggEx.InnerExceptions)
                    {
                        logger.Error($"Exception: {ex}");
                    }
                }

                btn.Text = "_OK";
            })
            .Forget(ex =>
            {
                logger.Error($"Exception: {ex}");
            });
    }
}
