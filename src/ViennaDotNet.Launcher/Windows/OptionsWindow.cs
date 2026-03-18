using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace ViennaDotNet.Launcher.Windows;

internal sealed class OptionsWindow : Window
{
    private TextField _apiPortInput;
    private TextField _eventBusPortInput;
    private TextField _objectStorePortInput;
    private TextField _thisIPInput;

    private CheckBox _enableTileRenderingInput;
    private RadioGroup _tileDataSourceInput;
    private TextField _mapTilerApiKeyInput;
    private TextField _tileDBConnectionInput;

    private CheckBox _generatePreviewOnImportInput;
    private CheckBox _skipFileValidationInput;

    private TextField _earthDBConnectionInput;
    private TextField _liveDBConnectionInput;

    public OptionsWindow(Settings settings)
    {
        const int ChoiceButtonsGroup = 0;

        Title = "Options";

        var tabs = new TabView()
        {
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(2),
        };

        AddNetworkTab(tabs, settings);
        AddMapTab(tabs, settings);
        AddDataTab(tabs, settings);
        AddDatabaseTab(tabs, settings);

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

            if (!ushort.TryParse(_apiPortInput.Text, out ushort apiPort))
            {
                MessageBox.ErrorQuery("Error", $"Network/Api port is invalid, must be integer between 0 and {ushort.MaxValue}", "OK");
            }
            else if (!ushort.TryParse(_eventBusPortInput.Text, out ushort eventBusPort))
            {
                MessageBox.ErrorQuery("Error", $"Network/EventBus port is invalid, must be integer between 0 and {ushort.MaxValue}", "OK");
            }
            else if (!ushort.TryParse(_objectStorePortInput.Text, out ushort objectStorePort))
            {
                MessageBox.ErrorQuery("Error", $"Network/ObjectStore port is invalid, must be integer between 0 and {ushort.MaxValue}", "OK");
            }
            else if (!IPAddress.TryParse(_thisIPInput.Text, out var thisIP) || thisIP.AddressFamily is not System.Net.Sockets.AddressFamily.InterNetwork)
            {
                MessageBox.ErrorQuery("Error", $"Network/IPv4 is invalid, must be a valid IPv4 address", "OK");
            }
            else if (string.IsNullOrWhiteSpace(_earthDBConnectionInput.Text))
            {
                MessageBox.ErrorQuery("Error", "Database/Earth database connection string is invalid", "OK");
            }
            else if (string.IsNullOrWhiteSpace(_tileDBConnectionInput.Text))
            {
                MessageBox.ErrorQuery("Error", "Database/Tile database connection string is invalid", "OK");
            }
            else
            {
                if (_enableTileRenderingInput.CheckedState is CheckState.Checked)
                {
                    switch ((Settings.TileDataSourceEnum)_tileDataSourceInput.SelectedItem)
                    {
                        case Settings.TileDataSourceEnum.MapTiler:
                            if (string.IsNullOrWhiteSpace(_mapTilerApiKeyInput.Text))
                            {
                                MessageBox.ErrorQuery("Error", "Map/MapTiler API key is invalid", "OK");
                                return;
                            }

                            break;
                        case Settings.TileDataSourceEnum.PostgreSQL:
                            if (string.IsNullOrWhiteSpace(_tileDBConnectionInput.Text))
                            {
                                MessageBox.ErrorQuery("Error", "Map/Tile database connection string is invalid", "OK");
                                return;
                            }

                            break;
                        default:
                            Debug.Fail($"Unknown {nameof(Settings.TileDataSourceEnum)} '{_tileDataSourceInput.SelectedItem}'");
                            break;
                    }
                }

                settings.ApiPort = apiPort;
                settings.EventBusPort = eventBusPort;
                settings.ObjectStorePort = objectStorePort;
                settings.IPv4 = thisIP.ToString();

                settings.EnableTileRenderingLabel = _enableTileRenderingInput.CheckedState switch
                {
                    CheckState.None => false,
                    CheckState.Checked => true,
                    CheckState.UnChecked => false,
                    _ => false,
                };
                settings.TileDataSource = (Settings.TileDataSourceEnum)_tileDataSourceInput.SelectedItem;
                settings.MapTilerApiKey = _mapTilerApiKeyInput.Text.Trim();
                settings.TileDatabaseConnectionString = _tileDBConnectionInput.Text.Trim();

                settings.GeneratePreviewOnImport = _generatePreviewOnImportInput.CheckedState switch
                {
                    CheckState.None => false,
                    CheckState.Checked => true,
                    CheckState.UnChecked => false,
                    _ => false,
                };
                settings.SkipFileChecks = _skipFileValidationInput.CheckedState switch
                {
                    CheckState.None => false,
                    CheckState.Checked => true,
                    CheckState.UnChecked => false,
                    _ => false,
                };

                settings.EarthDatabaseConnectionString = _earthDBConnectionInput.Text.Trim();
                settings.TileDatabaseConnectionString = _tileDBConnectionInput.Text.Trim();

                Application.RequestStop();
            }
        };

        Add(tabs,
            cancelBtn, applyBtn);
    }

    [MemberNotNull(nameof(_apiPortInput), nameof(_eventBusPortInput), nameof(_objectStorePortInput), nameof(_thisIPInput))]
    private void AddNetworkTab(TabView tabs, Settings settings)
    {
        var apiPortLabel = new Label()
        {
            Text = "Api _port:"
        };

        _apiPortInput = new TextField()
        {
            Text = settings.ApiPort.ToString(),
            X = Pos.Right(apiPortLabel) + 1,
            Y = Pos.Y(apiPortLabel),
            Width = Dim.Fill(),
        };

        var eventBusPortLabel = new Label()
        {
            Text = "_EventBus port:",
            X = Pos.Left(apiPortLabel),
            Y = Pos.Bottom(apiPortLabel) + 1,
        };

        _eventBusPortInput = new TextField()
        {
            Text = settings.EventBusPort.ToString(),
            X = Pos.Right(eventBusPortLabel) + 1,
            Y = Pos.Y(eventBusPortLabel),
            Width = Dim.Fill(),
        };

        var objectStorePortLabel = new Label()
        {
            Text = "_ObjectStore port:",
            X = Pos.Left(eventBusPortLabel),
            Y = Pos.Bottom(eventBusPortLabel) + 1,
        };

        _objectStorePortInput = new TextField()
        {
            Text = settings.ObjectStorePort.ToString(),
            X = Pos.Right(objectStorePortLabel) + 1,
            Y = Pos.Y(objectStorePortLabel),
            Width = Dim.Fill(),
        };

        var thisIPLabel = new Label()
        {
            Text = "_IPv4 (IP of this computer):",
            X = Pos.Left(objectStorePortLabel),
            Y = Pos.Bottom(objectStorePortLabel) + 1,
        };

        _thisIPInput = new TextField()
        {
            Text = settings.IPv4,
            X = Pos.Right(thisIPLabel) + 1,
            Y = Pos.Y(thisIPLabel),
            Width = Dim.Fill(),
        };

        var tab = new Tab()
        {
            DisplayText = "_Network",
            View = new View { Width = Dim.Fill(), Height = Dim.Fill(), CanFocus = true }
        };

        tab.View.Add(apiPortLabel, _apiPortInput,
            eventBusPortLabel, _eventBusPortInput,
            objectStorePortLabel, _objectStorePortInput,
            thisIPLabel, _thisIPInput);

        tabs.AddTab(tab, true);
    }

    [MemberNotNull(nameof(_enableTileRenderingInput), nameof(_tileDataSourceInput), nameof(_mapTilerApiKeyInput), nameof(_tileDBConnectionInput))]
    private void AddMapTab(TabView tabs, Settings settings)
    {
        var enableTileRenderingLabel = new Label()
        {
            Text = "_Enable tile rendering:",
        };

        _enableTileRenderingInput = new CheckBox()
        {
            Text = settings.EnableTileRenderingLabel switch
            {
                true => "Yes",
                false => "No",
                _ => "",
            },
            CheckedState = settings.EnableTileRenderingLabel switch
            {
                true => CheckState.Checked,
                false => CheckState.UnChecked,
                _ => CheckState.None,
            },
            X = Pos.Right(enableTileRenderingLabel) + 1,
            Y = Pos.Y(enableTileRenderingLabel),
        };

        var tileDataSourceLabel = new Label()
        {
            Text = "Tile data source:",
            X = Pos.Left(enableTileRenderingLabel),
            Y = Pos.Bottom(enableTileRenderingLabel) + 1
        };

        _tileDataSourceInput = new RadioGroup()
        {
            RadioLabels = ["Map_Tiler", "_PostgreSQL"],
            X = Pos.Right(tileDataSourceLabel) + 1,
            Y = Pos.Y(tileDataSourceLabel),
            Width = Dim.Fill(),
            Enabled = settings.EnableTileRenderingLabel is true,
        };
        _tileDataSourceInput.SelectedItem = (int)(settings.TileDataSource ?? Settings.TileDataSourceEnum.MapTiler);

        var mapTilerApiKeyLabel = new Label()
        {
            Text = "MapTiler api _key:",
            X = Pos.Left(tileDataSourceLabel),
            Y = Pos.Bottom(_tileDataSourceInput) + 1,
        };

        _mapTilerApiKeyInput = new TextField()
        {
            Text = settings.MapTilerApiKey ?? "",
            X = Pos.Right(mapTilerApiKeyLabel) + 1,
            Y = Pos.Y(mapTilerApiKeyLabel),
            Width = Dim.Fill(),
            Enabled = settings.EnableTileRenderingLabel is true,
        };

        var tileDBConnectionLabel = new Label()
        {
            Text = "Tile database _connection string:",
            X = Pos.Left(tileDataSourceLabel),
            Y = Pos.Bottom(_tileDataSourceInput) + 1,
        };

        _tileDBConnectionInput = new TextField()
        {
            Text = settings.TileDatabaseConnectionString,
            X = Pos.Right(tileDBConnectionLabel) + 1,
            Y = Pos.Y(tileDBConnectionLabel),
            Width = Dim.Fill(),
            Enabled = settings.EnableTileRenderingLabel is true,
        };

        _enableTileRenderingInput.CheckedStateChanged += (s, e) =>
        {
            _enableTileRenderingInput.Text = e.Value switch
            {
                CheckState.Checked => "Yes",
                CheckState.UnChecked => "No",
                _ => "",
            };

            _tileDataSourceInput.Enabled = e.Value == CheckState.Checked;
            _mapTilerApiKeyInput.Enabled = e.Value == CheckState.Checked;
            _tileDBConnectionInput.Enabled = e.Value == CheckState.Checked;
        };

        _tileDataSourceInput.SelectedItemChanged += (s, e) => UpdateTileDataSource();
        UpdateTileDataSource();

        var tab = new Tab()
        {
            DisplayText = "_Map",
            View = new View { Width = Dim.Fill(), Height = Dim.Fill(), CanFocus = true }
        };
        tab.View.Add(enableTileRenderingLabel, _enableTileRenderingInput,
            tileDataSourceLabel, _tileDataSourceInput,
            mapTilerApiKeyLabel, _mapTilerApiKeyInput,
            tileDBConnectionLabel, _tileDBConnectionInput);

        tabs.AddTab(tab, false);

        void UpdateTileDataSource()
        {
            mapTilerApiKeyLabel.Enabled = mapTilerApiKeyLabel.Visible = _tileDataSourceInput.SelectedItem == 0;
            _mapTilerApiKeyInput.Enabled = _mapTilerApiKeyInput.Visible = _tileDataSourceInput.SelectedItem == 0;

            tileDBConnectionLabel.Enabled = tileDBConnectionLabel.Visible = _tileDataSourceInput.SelectedItem == 1;
            _tileDBConnectionInput.Enabled = _tileDBConnectionInput.Visible = _tileDataSourceInput.SelectedItem == 1;
        }
    }

    [MemberNotNull(nameof(_earthDBConnectionInput), nameof(_liveDBConnectionInput))]
    private void AddDatabaseTab(TabView tabs, Settings settings)
    {
        var earthDBConnectionLabel = new Label()
        {
            Text = "_Earth database connection string:",
        };

        _earthDBConnectionInput = new TextField()
        {
            Text = settings.EarthDatabaseConnectionString,
            X = Pos.Right(earthDBConnectionLabel) + 1,
            Y = Pos.Y(earthDBConnectionLabel),
            Width = Dim.Fill(),
        };

        var liveDBConnectionLabel = new Label()
        {
            Text = "_Live database connection string:",
            X = Pos.Left(earthDBConnectionLabel),
            Y = Pos.Bottom(earthDBConnectionLabel) + 1,
        };

        _liveDBConnectionInput = new TextField()
        {
            Text = settings.LiveDatabaseConnectionString,
            X = Pos.Right(liveDBConnectionLabel) + 1,
            Y = Pos.Y(liveDBConnectionLabel),
            Width = Dim.Fill(),
        };

        var tab = new Tab()
        {
            DisplayText = "Data_base",
            View = new View { Width = Dim.Fill(), Height = Dim.Fill(), CanFocus = true }
        };
        tab.View.Add(earthDBConnectionLabel, _earthDBConnectionInput,
            liveDBConnectionLabel, _liveDBConnectionInput);

        tabs.AddTab(tab, false);
    }

    [MemberNotNull(nameof(_generatePreviewOnImportInput), nameof(_skipFileValidationInput))]
    private void AddDataTab(TabView tabs, Settings settings)
    {
        var generatePreviewOnImportLabel = new Label()
        {
            Text = "_Generate buildplate preview on import:",
        };

        _generatePreviewOnImportInput = new CheckBox()
        {
            Text = settings.GeneratePreviewOnImport switch
            {
                true => "Yes",
                false => "No",
                _ => "",
            },
            CheckedState = settings.GeneratePreviewOnImport switch
            {
                true => CheckState.Checked,
                false => CheckState.UnChecked,
                _ => CheckState.None,
            },
            X = Pos.Right(generatePreviewOnImportLabel) + 1,
            Y = Pos.Y(generatePreviewOnImportLabel),
        };
        _generatePreviewOnImportInput.CheckedStateChanged += (s, e) =>
        {
            _generatePreviewOnImportInput.Text = e.Value switch
            {
                CheckState.Checked => "Yes",
                CheckState.UnChecked => "No",
                _ => "",
            };
        };

        var skipFileValidationLabel = new Label()
        {
            Text = "Skip file _validation before starting:",
            X = Pos.Left(generatePreviewOnImportLabel),
            Y = Pos.Bottom(generatePreviewOnImportLabel) + 1,
        };

        _skipFileValidationInput = new CheckBox()
        {
            Text = settings.SkipFileChecks switch
            {
                true => "Yes",
                false => "No",
                _ => "",
            },
            CheckedState = settings.SkipFileChecks switch
            {
                true => CheckState.Checked,
                false => CheckState.UnChecked,
                _ => CheckState.None,
            },
            X = Pos.Right(skipFileValidationLabel) + 1,
            Y = Pos.Y(skipFileValidationLabel),
        };
        _skipFileValidationInput.CheckedStateChanged += (s, e) =>
        {
            _skipFileValidationInput.Text = e.Value switch
            {
                CheckState.Checked => "Yes",
                CheckState.UnChecked => "No",
                _ => "",
            };
        };

        var tab = new Tab()
        {
            DisplayText = "_Data",
            View = new View { Width = Dim.Fill(), Height = Dim.Fill(), CanFocus = true }
        };
        tab.View.Add(generatePreviewOnImportLabel, _generatePreviewOnImportInput,
            skipFileValidationLabel, _skipFileValidationInput);

        tabs.AddTab(tab, false);
    }
}
