using System.IO.Compression;
using System.Numerics;
using System.Runtime.InteropServices;
using ViennaDotNet.Buildplate.Model;

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
    [StructLayout(LayoutKind.Auto)]
    private readonly struct BlockState
    {
        public readonly string Name;
        public readonly bool IsAir => Name == "minecraft:air";
        public readonly bool IsMask => Name == "fountain:invisible_constraint";

        public readonly bool IsTransparent => IsAir || IsMask || Name.Contains("glass") || Name.Contains("leaves");
    }

    public static BuildplateMesh Generate(WorldData worldData)
    {
        // 1. Initialize a 3D grid for the buildplate.
        // Assuming a max height of 256 or 384 depending on the version. Let's use 256 for this example.
        int height = 256;
        BlockState[,,] voxelGrid = new BlockState[worldData.Size, height, worldData.Size];

        // 2. Extract Data from the Zip
        using (var ms = new MemoryStream(worldData.ServerData))
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Read))
        {
            foreach (var entry in archive.Entries)
            {
                if (entry.FullName.StartsWith("region/") && entry.Name.EndsWith(".mca"))
                {
                    ProcessRegionFile(entry, voxelGrid, worldData);
                }
            }
        }

        // 3. Generate the Mesh via Face Culling
        return BuildMesh(voxelGrid, worldData.Size, height);
    }

    private static void ProcessRegionFile(ZipArchiveEntry mcaEntry, BlockState[,,] voxelGrid, WorldData config)
    {
        using var stream = mcaEntry.Open();
        using var reader = new BinaryReader(stream);

        // Read the 4KB offset table (1024 chunks max per region, 4 bytes per chunk)
        for (int i = 0; i < 1024; i++)
        {
            byte[] offsetData = reader.ReadBytes(4);
            int offset = (offsetData[0] << 16) | (offsetData[1] << 8) | offsetData[2];
            int sectorCount = offsetData[3];

            if (offset == 0 && sectorCount == 0) continue; // Chunk not generated

            // Note: In a real implementation, you'd seek to (offset * 4096).
            // Since ZipStreams usually don't support Seek, you might need to copy the MCA to a memory stream first.

            // --> [SEEK TO OFFSET * 4096] <--
            // int length = reader.ReadInt32(); // Big Endian
            // byte compressionType = reader.ReadByte(); 
            // byte[] compressedNbt = reader.ReadBytes(length - 1);

            // Decompress (Zlib/GZip depending on compressionType) and pass to SharpNBT:
            // using var nbtStream = new MemoryStream(Decompress(compressedNbt));
            // var chunkCompound = CompoundTag.Read(nbtStream);

            // Extract sections, read the Palette and block_states array, 
            // and populate `voxelGrid[x, y, z]`
            // (Ignoring exact bit-unpacking logic here for brevity)
        }
    }

    private static BuildplateMesh BuildMesh(BlockState[,,] grid, int size, int height)
    {
        var vertices = new List<VoxelVertex>();
        var indices = new List<uint>();
        uint vertexCount = 0;

        // Loop every block in the buildplate
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    BlockState currentBlock = grid[x, y, z];

                    // If it's air or our special mask block, we DO NOT render it.
                    if (currentBlock.IsAir || currentBlock.IsMask)
                        continue;

                    float texIndex = GetTextureIndex(currentBlock.Name);

                    // --- CULLING LOGIC ---
                    // We only generate a face if the adjacent block is transparent/air/mask.

                    // +Y (Top)
                    if (y == height - 1 || grid[x, y + 1, z].IsTransparent)
                        AddFace(vertices, indices, ref vertexCount, x, y, z, Vector3.UnitY, texIndex);

                    // -Y (Bottom)
                    if (y == 0 || grid[x, y - 1, z].IsTransparent)
                        AddFace(vertices, indices, ref vertexCount, x, y, z, -Vector3.UnitY, texIndex);

                    // +X (Right)
                    if (x == size - 1 || grid[x + 1, y, z].IsTransparent)
                        AddFace(vertices, indices, ref vertexCount, x, y, z, Vector3.UnitX, texIndex);

                    // -X (Left)
                    if (x == 0 || grid[x - 1, y, z].IsTransparent)
                        AddFace(vertices, indices, ref vertexCount, x, y, z, -Vector3.UnitX, texIndex);

                    // +Z (Front)
                    if (z == size - 1 || grid[x, y, z + 1].IsTransparent)
                        AddFace(vertices, indices, ref vertexCount, x, y, z, Vector3.UnitZ, texIndex);

                    // -Z (Back)
                    if (z == 0 || grid[x, y, z - 1].IsTransparent)
                        AddFace(vertices, indices, ref vertexCount, x, y, z, -Vector3.UnitZ, texIndex);
                }
            }
        }

        return new BuildplateMesh
        {
            Vertices = vertices.ToArray(),
            Indices = indices.ToArray()
        };
    }

    private static void AddFace(List<VoxelVertex> vertices, List<uint> indices, ref uint vCount, int x, int y, int z, Vector3 normal, float texIndex)
    {
        Span<Vector3> corenrs = stackalloc Vector3[4];
        GetFaceCorners(x, y, z, normal, corenrs);

        // 2. Add Vertices
        vertices.Add(new VoxelVertex(corners[0], normal, new Vector2(0, 0), texIndex));
        vertices.Add(new VoxelVertex(corners[1], normal, new Vector2(1, 0), texIndex));
        vertices.Add(new VoxelVertex(corners[2], normal, new Vector2(1, 1), texIndex));
        vertices.Add(new VoxelVertex(corners[3], normal, new Vector2(0, 1), texIndex));

        indices.Add(vCount + 0); indices.Add(vCount + 1); indices.Add(vCount + 2);
        indices.Add(vCount + 2); indices.Add(vCount + 3); indices.Add(vCount + 0);

        vCount += 4;
    }

    private static float GetTextureIndex(string blockName)
    {
        return blockName switch
        {
            "minecraft:dirt" => 0f,
            "minecraft:stone" => 1f,
            "minecraft:grass_block" => 2f,
            _ => 3f // default/missing texture
        };
    }

    private static void GetFaceCorners(int x, int y, int z, Vector3 normal, Span<Vector3> corners)
    {
        // TODO: corners

        if (normal == Vector3.UnitY) return new[] {
            new Vector3(x, y + 1, z),
            new Vector3(x + 1, y + 1, z),
            new Vector3(x + 1, y + 1, z + 1),
            new Vector3(x, y + 1, z + 1)
        };

        // (You would implement the other 5 directions similarly...)
        return new Vector3[4];
    }
}