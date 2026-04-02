using System.Buffers;
using System.Collections.Frozen;
using System.Diagnostics;
using System.IO.Compression;
using System.Numerics;
using System.Runtime.InteropServices;
using BitcoderCZ.Maths.Vectors;
using SharpNBT;
using ViennaDotNet.Buildplate.Model;
using ViennaDotNet.BuildplateRenderer.Models.ResourcePacks;
using ViennaDotNet.BuildplateRenderer.Utils;
using ViennaDotNet.Common.Utils;

namespace ViennaDotNet.BuildplateRenderer;

[StructLayout(LayoutKind.Sequential)]
public struct VoxelVertex
{
    public Vector3 Position;
    public Vector3 Normal;
    public Vector2 UV;
    public float TextureIndex;

    public VoxelVertex(Vector3 pos, Vector3 norm, Vector2 uv, float texIndex)
    {
        Position = pos;
        Normal = norm;
        UV = uv;
        TextureIndex = texIndex;
    }
}

public class BuildplateMesh
{
    public VoxelVertex[] Vertices { get; set; }
    public uint[] Indices { get; set; }
}

internal sealed class MeshGenerator
{
    private static readonly FrozenSet<string> InvisibleBlocks = new HashSet<string>()
    {
        "minecraft:air",
        "fountain:solid_air",
        "fountain:non_replaceable_air",
        "fountain:invisible_constraint",
        "fountain:blend_constraint",
        "fountain:border_constraint",
    }.ToFrozenSet(StringComparer.Ordinal);

    private readonly ResourcePack _resourcePack;
    private readonly Random _rng = new();

    public MeshGenerator(ResourcePack resourcePack)
    {
        _resourcePack = resourcePack;
    }

    public async Task<BuildplateMesh> GenerateAsync(WorldData worldData, CancellationToken cancellationToken = default)
    {
        using (var serverDataStream = new MemoryStream(worldData.ServerData))
        using (var zip = await ZipArchive.CreateAsync(serverDataStream, ZipArchiveMode.Read, false, null, cancellationToken))
        {
            foreach (var entry in zip.Entries)
            {
                if (!entry.IsDirectory && entry.FullName.StartsWith("region"))
                {
                    var entryStream = await entry.OpenAsync(cancellationToken);
                    byte[] regionData = GC.AllocateUninitializedArray<byte>(checked((int)entry.Length));
                    await entryStream.ReadExactlyAsync(regionData, cancellationToken);

                    ProcessRegion(regionData, RegionUtils.PathToPos(entry.FullName));
                }
            }
        }

        // todo: use the official resourcepack
        throw new NotImplementedException();
    }

    private void ProcessRegion(byte[] regionData, int2 regionPosition)
    {
        foreach (var localPosition in RegionUtils.GetChunkPositions(regionData))
        {
            var chunkNBT = RegionUtils.ReadChunkNTB(regionData, localPosition);

            ProcessChunk(chunkNBT, RegionUtils.LocalToChunk(localPosition, regionPosition));
        }
    }

    // https://minecraft.wiki/w/Chunk_format
    private void ProcessChunk(CompoundTag nbt, int2 chunkPosition)
    {
        Debug.Assert(((IntTag)nbt["xPos"]).Value == chunkPosition.X);
        Debug.Assert(((IntTag)nbt["zPos"]).Value == chunkPosition.Y);

        foreach (var item in (ListTag)nbt["sections"])
        {
            var subChunkNBT = (CompoundTag)item;
            if (!subChunkNBT.ContainsKey("block_states"))
            {
                continue;
            }

            ProcessSubChunk(subChunkNBT, new int3(chunkPosition.X, ((ByteTag)subChunkNBT["Y"]).Value, chunkPosition.Y));
        }
    }

    private void ProcessSubChunk(CompoundTag nbt, int3 chunkPosition)
    {
        var blockStates = (CompoundTag)nbt["block_states"];

        var palette = (ListTag)blockStates["palette"];

        bool foundVisibleBlock = false;
        foreach (var entry in palette)
        {
            if (!InvisibleBlocks.Contains(((StringTag)((CompoundTag)entry)["Name"]).Value))
            {
                foundVisibleBlock = true;
                break;
            }
        }

        if (!foundVisibleBlock)
        {
            return;
        }

        var blocks = blockStates.ContainsKey("data")
            ? ChunkUtils.ReadBlockData((LongArrayTag)blockStates["data"])
            : ChunkUtils.EmptySubChunk;

        var blockPosition = int3.Zero;

        var propertiesArray = ArrayPool<KeyValuePair<string, string>>.Shared.Rent(64);
        var modelVariants = ArrayPool<VariantModel>.Shared.Rent(64);

        foreach (var blockIndex in blocks)
        {
            Debug.Assert(blockPosition.X is >= 0 and < ChunkUtils.Width);
            Debug.Assert(blockPosition.Y is >= 0 and < ChunkUtils.SubChunkHeight);
            Debug.Assert(blockPosition.Z is >= 0 and < ChunkUtils.Width);

            var paletteEntry = (CompoundTag)palette[blockIndex];

            string blockName = ((StringTag)paletteEntry["Name"]).Value;

            if (!InvisibleBlocks.Contains(blockName))
            {
                if (blockName is "minecraft:water" or "minecraft:lava")
                {
                    // TODO:
                    continue;
                }

                int propertiesArrayLength = 0;
                if (paletteEntry.TryGetValue("Properties", out var propertiesTag))
                {
                    foreach (var item in (ICollection<KeyValuePair<string, Tag>>)(CompoundTag)propertiesTag)
                    {
                        if (item.Key is "waterlogged")
                        {
                            continue;
                        }

                        if (propertiesArrayLength >= propertiesArray.Length)
                        {
                            ArrayPool<KeyValuePair<string, string>>.Shared.Return(propertiesArray);
                            propertiesArray = ArrayPool<KeyValuePair<string, string>>.Shared.Rent(propertiesArray.Length * 2);
                        }

                        propertiesArray[propertiesArrayLength++] = new(item.Key, ((StringTag)item.Value).Value);
                    }
                }

                // todo: non allocating ctor, add a short PropertiesLength field
                var blockState = new Models.ResourcePacks.BlockState(blockName, propertiesArray.AsSpan()[..propertiesArrayLength]);

                var modelVariantsLength = _resourcePack.GetModelVariant(blockState, _rng, modelVariants);
                foreach (var modelVariant in modelVariants.AsSpan(0, modelVariantsLength))
                {
                    var model = _resourcePack.GetBlockModel(modelVariant.Model);
                }
            }

            blockPosition.X++;
            if (blockPosition.X >= ChunkUtils.Width)
            {
                blockPosition.X = 0;
                blockPosition.Z++;
                if (blockPosition.Z >= 16)
                {
                    blockPosition.Z = 0;
                    blockPosition.Y++;
                }
            }
        }

        ArrayPool<KeyValuePair<string, string>>.Shared.Return(propertiesArray);
        ArrayPool<VariantModel>.Shared.Return(modelVariants);
    }
}