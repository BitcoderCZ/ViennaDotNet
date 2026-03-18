using ViennaDotNet.Common.Buildplate.Connector.Model;

namespace ViennaDotNet.Buildplate.Connector.Model;

public sealed record PlayerDisconnectedRequest(
     string PlayerId,
     InventoryResponse? BackpackContents
);
