using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using ViennaDotNet.DB;
using ViennaDotNet.DB.Models.Player;
using ViennaDotNet.Launcher.Utils;

namespace ViennaDotNet.Launcher.Windows.Data;

internal sealed class PlayerWindow : Window
{
    private readonly EarthDB _db;

    public PlayerWindow(string playerId, string? username, EarthDB db)
    {
        _db = db;

        Title = $"Manage data/Players/{playerId}";

        UIUtils.Load(this, async cancellationToken =>
        {
            var results = await new EarthDB.Query(false)
                .Get("profile", playerId, typeof(Profile))
                .ExecuteAsync(_db, cancellationToken);

            var profile = results.Get<Profile>("profile");

            var idLabel = new Label()
            {
                Text = $"Id: {playerId}" + (username is null ? "" : (", Username: " + username)),
            };

            var rubiesLabel = new Label()
            {
                Text = "Rubies:",
                X = Pos.Left(idLabel),
                Y = Pos.Bottom(idLabel) + 1,
            };

            var rubiesPurchasedLabel = new Label()
            {
                Text = "_Purchased:",
                X = Pos.Left(rubiesLabel) + 3,
                Y = Pos.Bottom(rubiesLabel),
            };

            var rubiesPurchasedInput = new NumericUpDown()
            {
                Value = profile.Rubies.Purchased,
                X = Pos.Right(rubiesPurchasedLabel) + 1,
                Y = Pos.Y(rubiesPurchasedLabel),
            };
            rubiesPurchasedInput.ValueChanging += (s, e) =>
            {
                if (e.NewValue < 0)
                {
                    e.Cancel = true;
                    rubiesPurchasedInput.Value = 0;
                }
                else
                {
                    profile.Rubies.Purchased = e.NewValue;
                }
            };

            var rubiesPurchasedAdd = new Button()
            {
                Text = "+100",
                IsDefault = false,
                X = Pos.Right(rubiesPurchasedInput) + 1,
                Y = Pos.Y(rubiesPurchasedInput),
            };
            rubiesPurchasedAdd.Accepting += (s, e) => rubiesPurchasedInput.Value += 100;

            var rubiesPurchasedRemove = new Button()
            {
                Text = "-100",
                IsDefault = false,
                X = Pos.Right(rubiesPurchasedAdd) + 1,
                Y = Pos.Y(rubiesPurchasedAdd),
            };
            rubiesPurchasedRemove.Accepting += (s, e) => rubiesPurchasedInput.Value -= 100;

            var rubiesEarnedLabel = new Label()
            {
                Text = "_Earned:",
                X = Pos.Left(rubiesPurchasedLabel),
                Y = Pos.Bottom(rubiesPurchasedLabel),
            };

            var rubiesEarnedInput = new NumericUpDown()
            {
                Value = profile.Rubies.Earned,
                X = Pos.X(rubiesPurchasedInput),
                Y = Pos.Y(rubiesEarnedLabel),
            };
            rubiesEarnedInput.ValueChanging += (s, e) =>
            {
                if (e.NewValue < 0)
                {
                    e.Cancel = true;
                    rubiesEarnedInput.Value = 0;
                }
                else
                {
                    profile.Rubies.Earned = e.NewValue;
                }
            };

            var rubiesEarnedAdd = new Button()
            {
                Text = "+100",
                IsDefault = false,
                X = Pos.Right(rubiesEarnedInput) + 1,
                Y = Pos.Y(rubiesEarnedInput),
            };
            rubiesEarnedAdd.Accepting += (s, e) => rubiesEarnedInput.Value += 100;

            var rubiesEarnedRemove = new Button()
            {
                Text = "-100",
                IsDefault = false,
                X = Pos.Right(rubiesEarnedAdd) + 1,
                Y = Pos.Y(rubiesEarnedAdd),
            };
            rubiesEarnedRemove.Accepting += (s, e) => rubiesEarnedInput.Value -= 100;

            var saveBtn = new Button()
            {
                Text = "_Save",
                Y = Pos.Bottom(rubiesEarnedRemove),
            };
            saveBtn.Accepting += async (s, e) =>
            {
                e.Handled = true;

                try
                {
                    await new EarthDB.Query(true)
                        .Update("profile", playerId, profile)
                        .ExecuteAsync(_db);

                    Application.RequestStop(this);
                }
                catch (EarthDB.DatabaseException ex)
                {
                    MessageBox.ErrorQuery("Error", $"Failed to save: {ex.GetInnerMostMessage()}", "OK");
                }
            };

            var cancelBtn = new Button()
            {
                Text = "_Cancel",
                Y = Pos.Bottom(saveBtn),
            };
            cancelBtn.Accepting += (s, e) =>
            {
                e.Handled = true;

                Application.RequestStop(this);
            };

            Add(idLabel,
                rubiesLabel,
                    rubiesPurchasedLabel, rubiesPurchasedInput, rubiesPurchasedAdd, rubiesPurchasedRemove,
                    rubiesEarnedLabel, rubiesEarnedInput, rubiesEarnedAdd, rubiesEarnedRemove,
                saveBtn,
                cancelBtn);
        });
    }
}
