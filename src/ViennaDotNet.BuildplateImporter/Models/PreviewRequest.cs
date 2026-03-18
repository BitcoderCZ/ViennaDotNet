namespace ViennaDotNet.BuildplateImporter.Models;

internal sealed record PreviewRequest(
    string serverDataBase64,
    bool night
);