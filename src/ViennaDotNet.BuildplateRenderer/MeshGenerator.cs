using System.Collections.Frozen;
using System.Diagnostics;
using System.IO.Compression;
using System.Numerics;
using System.Runtime.InteropServices;
using BitcoderCZ.Maths.Vectors;
using SharpNBT;
using ViennaDotNet.Buildplate.Model;
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

internal static class MeshGenerator
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

    public static async Task<BuildplateMesh> GenerateAsync(WorldData worldData, CancellationToken cancellationToken = default)
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

    private static void ProcessRegion(byte[] regionData, int2 regionPosition)
    {
        foreach (var localPosition in RegionUtils.GetChunkPositions(regionData))
        {
            var chunkNBT = RegionUtils.ReadChunkNTB(regionData, localPosition);

            ProcessChunk(chunkNBT, RegionUtils.LocalToChunk(localPosition, regionPosition));
        }
    }

    // https://minecraft.wiki/w/Chunk_format
    private static void ProcessChunk(CompoundTag nbt, int2 chunkPosition)
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

    private static void ProcessSubChunk(CompoundTag nbt, int3 chunkPosition)
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

        foreach (var blockIndex in blocks)
        {
            Debug.Assert(blockPosition.X is >= 0 and < ChunkUtils.Width);
            Debug.Assert(blockPosition.Y is >= 0 and < ChunkUtils.SubChunkHeight);
            Debug.Assert(blockPosition.Z is >= 0 and < ChunkUtils.Width);

            var paletteEntry = (CompoundTag)palette[blockIndex];

            string blockName = ((StringTag)paletteEntry["Name"]).Value;

            if (!InvisibleBlocks.Contains(blockName))
            {
                
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
    }
}