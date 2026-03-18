using Serilog;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using ViennaDotNet.Common.Utils;
using ViennaDotNet.DB;
using ViennaDotNet.DB.Models.Player;
using ViennaDotNet.Launcher.Utils;
using ViennaDotNet.Launcher.Views;

using FullProfile = (string Id, string? Username, ViennaDotNet.DB.Models.Player.Profile Profile);

namespace ViennaDotNet.Launcher.Windows.Data;

internal sealed class PlayersWindow : Window
{
    private readonly EarthDB _db;

    public PlayersWindow(Window parent, EarthDB db, Settings settings)
    {
        _db = db;

        Title = parent.Title + "/Players";

        //editBtn.Accepting += async (s, e) =>
        //{
        //    e.Handled = true;

        //    if (await DataUtils.GetPlayerCountAsync(_db) is null or 0)
        //    {
        //        MessageBox.ErrorQuery("No players", "There are no players in the database.", "OK");
        //        return;
        //    }

        //    string? selected = SelectDialog.Show("Select player to edit", DataUtils.GetFullProfilesAsync(_db, liveDb).Select(item => $"{item.Id}{(item.Username is not null ? $" \"{item.Username}\" " : " ")}{item.Profile.Level}LV {item.Profile.Rubies.Total} Rubies").ToBlockingEnumerable());

        //    if (selected is null)
        //    {
        //        return;
        //    }

        //    // TODO: get id and username in a better way
        //    Application.Run(new PlayerWindow(selected[..selected.IndexOf(' ')], selected.Contains('"') ? selected[(selected.IndexOf('"') + 1)..selected.LastIndexOf('"')] : null, _db));
        //};

        var backBtn = new Button()
        {
            Text = "_Back",
            X = 1,
            Y = 1,
        };
        backBtn.Accepting += (s, e) =>
        {
            e.Handled = true;

            Application.RequestStop(this);
        };

        var removeBtn = new Button()
        {
            Text = "_Remove",
            X = Pos.Right(backBtn) + 1,
            Y = Pos.Y(backBtn),
        };
        removeBtn.Accepting += (s, e) =>
        {
            e.Handled = true;
        };

        var editLabel = new Label()
        {
            Text = "Edit:",
            X = Pos.Center(),
            Y = Pos.Bottom(removeBtn) + 1,
        };

        Add(backBtn,
            removeBtn,
            editLabel);

        AddPlayerSelectionAsync(editLabel, settings)
            .Forget(ex =>
            {
                Log.Error(ex.ToString());
                MessageBox.ErrorQuery("Error", ex.GetInnerMostMessage(), "OK");
            });
    }

    private async Task AddPlayerSelectionAsync(View lastView, Settings settings)
    {
        if (await DataUtils.GetPlayerCountAsync(_db) is null or 0)
        {
            var infoLabel = new Label()
            {
                Text = "There are no players in the database.",
                X = Pos.Center(),
                Y = Pos.Bottom(lastView) + 1,
            };

            Add(infoLabel);
            return;
        }

        var frame = new FrameView()
        {
            CanFocus = true,
            Y = Pos.Bottom(lastView) + 1,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };

        var playerSelection = new TabbedListView<ToStringWrapper<FullProfile>>(new PlayerDataProvider(_db, settings))
        {
            CanFocus = true,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };
        playerSelection.OnItemSelected += (item, index) =>
        {
            var profile = item.Value;

            Application.Run(new PlayerWindow(profile.Id, profile.Username, _db));
        };

        frame.Add(playerSelection);

        Add(frame);
    }

    private sealed class PlayerDataProvider : IDataProvider<ToStringWrapper<FullProfile>>
    {
        private readonly EarthDB _db;
        private readonly Settings _settings;

        public PlayerDataProvider(EarthDB db, Settings settings)
        {
            _db = db;
            _settings = settings;
        }

        public int? Count => (int?)DataUtils.GetPlayerCount(_db);

        public IEnumerable<ToStringWrapper<(string Id, string? Username, Profile Profile)>> GetData(int skip, int count)
            => GetDataAsync(skip, count).ToBlockingEnumerable();

        public IAsyncEnumerable<ToStringWrapper<(string Id, string? Username, Profile Profile)>> GetDataAsync(int skip, int count, CancellationToken cancellationToken = default)
        {
            using var liveDb = DataUtils.OpenLiveDB(_settings);

            return DataUtils.GetFullProfilesAsync(_db, liveDb, cancellationToken)
                .Select(profile => new ToStringWrapper<FullProfile>(profile, ProfileToString));
        }

        private static string ProfileToString(FullProfile profile)
            => $"{profile.Id}{(profile.Username is not null ? $" \"{profile.Username}\" " : " ")}{profile.Profile.Level}LV {profile.Profile.Rubies.Total} Rubies";
    }
}