using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using ViennaDotNet.DB;
using ViennaDotNet.Launcher.Utils;
using ViennaDotNet.Launcher.Windows.Data;

namespace ViennaDotNet.Launcher.Windows;

internal sealed class DataWindow : Window
{
    private readonly EarthDB _db;

    public DataWindow(EarthDB db, Settings settings)
    {
        _db = db;

        Title = "Manage data";

        UIUtils.Load(this, async cancellationToken =>
        {
            var playersBtn = new Button()
            {
                X = Pos.Center(),
                Y = Pos.Absolute(1),
                Text = $"_Players [{(await DataUtils.GetPlayerCountAsync(_db, cancellationToken))?.ToString() ?? "?"}]",
            };
            playersBtn.Accepting += (s, e) =>
            {
                e.Handled = true;

                using var window = new PlayersWindow(this, _db, settings)
                {
                    X = Pos.Center(),
                    Y = Pos.Center(),
                    //Modal = true,
                };

                Application.Run(window);
            };

            var backBtn = new Button()
            {
                Text = "_Back",
                X = Pos.Center(),
                Y = Pos.Bottom(playersBtn) + 1,
            };
            backBtn.Accepting += (s, e) =>
            {
                e.Handled = true;

                Application.RequestStop(this);
            };

            Add(playersBtn,
                backBtn);

            playersBtn.SetFocus();
        });
    }
}
