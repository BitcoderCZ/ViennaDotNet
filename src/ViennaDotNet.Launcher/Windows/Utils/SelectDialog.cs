using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace ViennaDotNet.Launcher.Windows.Utils;

internal sealed class SelectDialog : Window
{
    public SelectDialog(string title, IEnumerable<string> options)
    {
        Title = title;

        Width = Dim.Fill();
        Height = Dim.Fill();

        var frameView = new FrameView()
        {
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(2),
        };

        var radioGroup = new RadioGroup
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            RadioLabels = [.. options],
            Orientation = Orientation.Vertical,
        };
        radioGroup.SelectedItem = -1;

        radioGroup.SelectedItemChanged += (s, e) =>
        {
            if (e.SelectedItem is not { } selectedItem)
            {
                return;
            }

            Selected = radioGroup.RadioLabels[selectedItem];

            Application.RequestStop(this);
        };
        frameView.Add(radioGroup);

        var cancelBtn = new Button()
        {
            Text = "_Cancel",
            X = Pos.Center(),
            Y = Pos.AnchorEnd(),
        };

        cancelBtn.Accepting += (s, e) =>
        {
            e.Handled = true;

            Application.RequestStop(this);
        };

        Add(frameView,
            cancelBtn);
    }

    public string? Selected { get; private set; }

    public static string? Show(string title, IEnumerable<string> options)
    {
        using var dialog = new SelectDialog(title, options)
        {
            Modal = true,
        };

        Application.Run(dialog);

        return dialog.Selected;
    }
}
