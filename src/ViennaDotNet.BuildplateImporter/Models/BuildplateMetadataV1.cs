using System.Text.Json.Serialization;

namespace ViennaDotNet.BuildplateImporter.Models;

internal sealed record BuildplateMetadataV1(
    [property: JsonPropertyName("size")] int Size,
    [property: JsonPropertyName("offset")] int Offset,
    [property: JsonPropertyName("night")] bool Night
);