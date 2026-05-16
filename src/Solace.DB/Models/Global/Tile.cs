namespace Solace.DB.Models.Global;

public sealed class Tile
{
    public ulong Id { get; set; }

    public required string ObjectStoreId { get; set; }
}