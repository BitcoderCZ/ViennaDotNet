using SkiaSharp;
using ViennaDotNet.Buildplate.Model;

namespace ViennaDotNet.BuildplateRenderer;

public sealed class Renderer
{
    private readonly SKCanvas _canvas;

    private Renderer(SKCanvas canvas)
    {
        _canvas = canvas;
    }

    public static Renderer CreateForBuildplate(WorldData worldData, SKCanvas canvas)
    {
        

        return new Renderer(canvas);
    }
}