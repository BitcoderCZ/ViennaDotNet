namespace ViennaDotNet.BuildplateImporter;

internal sealed record WorldData(
    byte[] ServerData,
    int Size,
    int Offset,
    bool Night
);