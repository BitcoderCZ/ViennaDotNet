using System.Text.Json.Serialization;

namespace ViennaDotNet.BuildplateImporter.Models;

internal sealed record BuildplateMetadataVersion(
    [property: JsonPropertyName("version")] int Version
);