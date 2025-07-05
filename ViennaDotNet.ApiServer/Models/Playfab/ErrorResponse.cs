using System.Text.Json.Serialization;

namespace ViennaDotNet.ApiServer.Models.Playfab;

internal sealed record ErrorResponse(
    int Code,
    string Status,
    string Error,
    int ErrorCode,
    string ErrorMessage,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] Dictionary<string, string[]>? ErrorDetails
);