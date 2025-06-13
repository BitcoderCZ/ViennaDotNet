namespace ViennaDotNet.TileRenderer.Wkb;

internal sealed class Tile
{
    public Tile(Point slippy, int zoom, int resolution)
    {
        Slippy = slippy;
        Zoom = zoom;
        Resolution = resolution;
    }

    public Point Slippy { get; }

    public int Zoom { get; }

    public int Resolution { get; }

    public Point ToLocalPixel(Point sphereMerc)
    {
        //printf("Converting point: %lf, %lf\n", sphereMerc.x, sphereMerc.y);
        Point slippy = TileUtils.SphereMercToSlippy(sphereMerc, Zoom);
        //printf("Offsetting point: %lf, %lf\n", slippy.x, slippy.y);
        slippy -= Slippy;
        slippy *= Resolution;
        //printf("Rendering point: %lf, %lf\n\n", slippy.x, slippy.y);
        return slippy;
    }
}
