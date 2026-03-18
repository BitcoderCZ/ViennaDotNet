namespace ViennaDotNet.StaticData;

public sealed class StaticData
{
    private readonly string _directory;

    private Catalog? _catalog;
    private PlayerLevels? _levels;
    private TappablesConfig? _tappablesConfig;
    private EncountersConfig? _encountersConfig;
    private TileRenderer? _tileRenderer;
    private Buildplates? _buildplates;
    private Playfab? _playfab;

    public StaticData(string dir)
    {
        _directory = Path.GetFullPath(dir);
    }

    public Catalog Catalog => _catalog ??= new Catalog(Path.Combine(_directory, "catalog"));

    public PlayerLevels Levels => _levels ??= new PlayerLevels(Path.Combine(_directory, "levels"));

    public TappablesConfig TappablesConfig => _tappablesConfig ??= new TappablesConfig(Path.Combine(_directory, "tappables"));

    public EncountersConfig EncountersConfig => _encountersConfig ??= new EncountersConfig(Path.Combine(_directory, "encounters"));

    public TileRenderer TileRenderer => _tileRenderer ??= new TileRenderer(Path.Combine(_directory, "tile_renderer"));

    public Buildplates Buildplates => _buildplates ??= new Buildplates(Path.Combine(_directory, "buildplates"));

    public Playfab Playfab => _playfab ??= new Playfab(Path.Combine(_directory, "playfab"));
}