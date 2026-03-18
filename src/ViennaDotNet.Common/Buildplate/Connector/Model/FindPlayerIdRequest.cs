namespace ViennaDotNet.Common.Buildplate.Connector.Model;

public sealed record FindPlayerIdRequest(
    string MinecraftId,
    string MinecraftName
);