using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Numerics;
using ViennaDotNet.BuildplateRenderer.Models.ResourcePacks;
using ViennaDotNet.BuildplateRenderer.Utils;

namespace ViennaDotNet.BuildplateRenderer;

// https://minecraft.wiki/w/Resource_pack
// https://minecraft.wiki/w/Model
public sealed class ResourcePack
{
    private readonly FrozenDictionary<string, BlockModel> _blockModels;

    public ResourcePack(FrozenDictionary<string, BlockModel> blockModels)
    {
        _blockModels = blockModels;
    }

    public static ResourcePack Load(DirectoryInfo rootDir)
    {
        var blockModelsDir = new DirectoryInfo(Path.Combine(rootDir.FullName, "models", "block"));
        var blockModelsJson = new Dictionary<string, BlockModelJson>();

        foreach (var file in blockModelsDir.EnumerateFiles())
        {
            string blockName = Path.GetFileNameWithoutExtension(file.Name);
            BlockModelJson model;
            using (var fs = File.OpenRead(file.FullName))
            {
                model = JsonUtils.DeserializeJson<BlockModelJson>(fs) ?? new();
            }

            blockModelsJson.Add(blockName, model);
        }

        var blockModels = new Dictionary<string, BlockModel>(blockModelsJson.Count);
        foreach (var (blokName, _) in blockModelsJson)
        {
            ResolveBlockModel(blokName);
        }

        return new ResourcePack(blockModels.ToFrozenDictionary());

        BlockModel ResolveBlockModel(string name)
        {
            if (name.Contains(':'))
            {
                Debug.Assert(name.StartsWith("minecraft:block/"));
                name = name["minecraft:block/".Length..];
            }
            else if (name.StartsWith("block/"))
            {
                name = name["block/".Length..];
            }

            if (blockModels.TryGetValue(name, out var existingModel))
            {
                return existingModel;
            }

            var json = blockModelsJson[name];

            var parent = json.Parent is null ? null : ResolveBlockModel(json.Parent);

            var textures = MergeDictionaries(json.Textures, parent?.Textures);

            ImmutableArray<BlockElement> elements;
            if (json.Elements is null)
            {
                if (parent?.Elements is null)
                {
                    elements = [];
                }
                else
                {
                    elements = parent.Elements;
                }
            }
            else
            {
                var elementBuilder = ImmutableArray.CreateBuilder<BlockElement>(json.Elements.Length);

                foreach (var element in json.Elements)
                {
                    var faces = new BlockElementFaces();
                    faces[0] = CreateBlockFace(element.Faces.East, element.From, element.To, 0);
                    faces[1] = CreateBlockFace(element.Faces.West, element.From, element.To, 1);
                    faces[2] = CreateBlockFace(element.Faces.Up, element.From, element.To, 2);
                    faces[3] = CreateBlockFace(element.Faces.Down, element.From, element.To, 3);
                    faces[4] = CreateBlockFace(element.Faces.South, element.From, element.To, 4);
                    faces[5] = CreateBlockFace(element.Faces.North, element.From, element.To, 5);

                    elementBuilder.Add(new BlockElement()
                    {
                        From = element.From,
                        To = element.To,
                        Shade = element.Shade,
                        LightEmission = element.LightEmission,
                        Faces = faces,
                    });
                }

                elements = elementBuilder.DrainToImmutable();
            }

            var model = new BlockModel()
            {
                Display = MergeDictionaries(json.Display, parent?.Display),
                Textures = textures,
                Elements = elements,
            };

            blockModels[name] = model;

            return model;
        }

        static BlockFace? CreateBlockFace(BlockFaceJson? json, Vector3 from, Vector3 to, int faceIndex)
        {
            if (json is null)
            {
                return null;
            }

            from /= 16f;
            to /= 16f;

            if (json.UV is not { } uv)
            {
                const float MaxValue = 1f;

                uv = faceIndex switch
                {
                    0 => new UVCoordinates(from.Z, MaxValue - to.Y, to.Z, MaxValue - from.Y),
                    1 => new UVCoordinates(MaxValue - to.Z, MaxValue - to.Y, MaxValue - from.Z, MaxValue - from.Y),
                    2 => new UVCoordinates(from.X, from.Z, to.X, to.Z),
                    3 => new UVCoordinates(from.X, MaxValue - to.Z, to.X, MaxValue - from.Z),
                    4 => new UVCoordinates(from.X, MaxValue - to.Y, to.X, MaxValue - from.Y),
                    5 => new UVCoordinates(MaxValue - to.X, MaxValue - to.Y, MaxValue - from.X, MaxValue - from.Y),
                    _ => new UVCoordinates(0, 0, MaxValue, MaxValue)
                };
            }

            Debug.Assert(json.Texture.StartsWith('#'));

            return new BlockFace()
            {
                UV = uv,
                Texture = json.Texture,
                CullFace = json.CullFace switch
                {
                    DirecionJson.East => Direcion.East,
                    DirecionJson.West => Direcion.West,
                    DirecionJson.Up or DirecionJson.Top => Direcion.Up,
                    DirecionJson.Down or DirecionJson.Bottom => Direcion.Down,
                    DirecionJson.South => Direcion.South,
                    DirecionJson.North => Direcion.North,
                    _ => throw new UnreachableException(),
                },
                Rotation = json.Rotation,
                TintIndex = json.TintIndex,
            };
        }
    }

    private static IReadOnlyDictionary<TKey, TValue> MergeDictionaries<TKey, TValue>(IReadOnlyDictionary<TKey, TValue>? @new, IReadOnlyDictionary<TKey, TValue>? @base)
        where TKey : notnull
    {
        if (@base is null or { Count: 0 })
        {
            return @new ?? new Dictionary<TKey, TValue>();
        }

        if (@new is null or { Count: 0 })
        {
            return @base ?? new Dictionary<TKey, TValue>();
        }

        var result = new Dictionary<TKey, TValue>(@new.Count + @base.Count);

        foreach (var (key, item) in @base)
        {
            result.Add(key, item);
        }

        foreach (var (key, item) in @new)
        {
            result[key] = item; // override base
        }

        return result;
    }
}