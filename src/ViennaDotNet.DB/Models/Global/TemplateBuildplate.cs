namespace ViennaDotNet.DB.Models.Global;

public sealed record TemplateBuildplate(
    int Size,
    int Offset,
    int Scale,
    bool Night,
    string ServerDataObjectId,
    string PreviewObjectId
);