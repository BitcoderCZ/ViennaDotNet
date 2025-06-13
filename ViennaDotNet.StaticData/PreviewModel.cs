namespace ViennaDotNet.PreviewGenerator;

internal sealed record PreviewModel(
     int format_version, // always 1
     bool isNight,
     PreviewModel.SubChunk[] sub_chunks,
     PreviewModel.BlockEntity[] blockEntities,
     PreviewModel.Entity[] entities
)
{
    public sealed record Position(
        int x,
        int y,
        int z
    );

    public sealed record SubChunk(
        Position position,
        SubChunk.PaletteEntry[] block_palette,
        int[] blocks
    )
    {
        public sealed record PaletteEntry(
            string name,
            int data
        );
    }

    public sealed record BlockEntity(
        int type,
        Position position,
        JsonNbtConverter.JsonNbtTag data
    );

    public sealed record Entity(
        string name,
        Entity.Position position,
        Entity.Rotation rotation,
        Entity.Position shadowPosition,
        float shadowSize,
        int overlayColor,
        int changeColor,
        int multiplicitiveTintChangeColor,
        Dictionary<string, object>? extraData,
        string skinData,
        bool isPersonaSkin
    )
    {
        public sealed record Position(
            float x,
            float y,
            float z
        );

        public sealed record Rotation(
            float x,
            float y
        );
    }
}
