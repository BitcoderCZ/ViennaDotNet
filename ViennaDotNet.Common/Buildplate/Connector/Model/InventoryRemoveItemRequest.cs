namespace ViennaDotNet.Buildplate.Connector.Model
{
    public record InventoryRemoveItemRequest(
         string playerId,
         string itemId,
         int count,
         string? instanceId
    )
    {
    }
}
