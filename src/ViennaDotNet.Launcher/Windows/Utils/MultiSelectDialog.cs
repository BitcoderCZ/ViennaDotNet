using System.Collections.ObjectModel;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace ViennaDotNet.Launcher.Windows.Utils;

internal sealed class MultiSelectDialog : Window
{
    public MultiSelectDialog(string title, IEnumerable<string> options)
    {
        const int ChoiceButtonsGroup = 0;

        Title = title;

        var frameView = new FrameView()
        {
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(2),
        };

        var listView = new ListView()
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };
        listView.VerticalScrollBar.AutoShow = true;
        listView.VerticalScrollBar.Enabled = true;
        listView.HorizontalScrollBar.AutoShow = true;
        listView.HorizontalScrollBar.Enabled = true;
        var optionsCollection = new ObservableCollection<string>(options);
        listView.SetSource(optionsCollection);
        frameView.Add(listView);

        var cancelBtn = new Button()
        {
            Text = "_Cancel",
            X = Pos.Align(Alignment.Center, AlignmentModes.AddSpaceBetweenItems, ChoiceButtonsGroup),
            Y = Pos.AnchorEnd(),
        };
        cancelBtn.Accepting += (s, e) =>
        {
            e.Handled = true;

            Application.RequestStop(this);
        };

        var applyBtn = new Button()
        {
            Text = "_Apply",
            X = Pos.Align(Alignment.Center, AlignmentModes.AddSpaceBetweenItems, ChoiceButtonsGroup),
            Y = Pos.AnchorEnd(),
        };
        applyBtn.Accepting += (s, e) =>
        {
            e.Handled = true;

            Selected = [.. optionsCollection.Where((_, index) => listView.Source.IsMarked(index))];

            Application.RequestStop(this);
        };

        Add(frameView,
            cancelBtn, applyBtn);
    }

    public IReadOnlyList<string>? Selected { get; private set; }

    public static IReadOnlyList<string>? Show(string title, IEnumerable<string> options)
    {
        using var dialog = new MultiSelectDialog(title, options)
        {
            Modal = true,
        };

        Application.Run(dialog);

        return dialog.Selected;
    }
}
