using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ViennaDotNet.BuildplateRenderer.Models.ResourcePacks;

// https://minecraft.wiki/w/Resource_pack#Texture_animation
public sealed class TextureInfoJson
{
    public required TextureAnimationJson Animation { get; init; }
}

public sealed class TextureAnimationJson
{
    public bool Interpolate { get; init; }

    public int? Width { get; init; }

    public int? Height { get; init; }

    [JsonPropertyName("frametime")]
    public int FrameTime { get; init; } = 1;

    public int[]? Frames { get; init; }
}

public sealed class TextureInfo
{
    public required TextureAnimation Animation { get; init; }
}

public sealed class TextureAnimation
{
    public bool Interpolate { get; init; }

    public int Width { get; init; }

    public int Height { get; init; }

    public int FrameTime { get; init; } = 1;

    public ImmutableArray<int> Frames { get; init; }
}