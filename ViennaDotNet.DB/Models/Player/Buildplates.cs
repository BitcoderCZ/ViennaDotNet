using System.Text.Json.Serialization;
using ViennaDotNet.Common.Utils;

namespace ViennaDotNet.DB.Models.Player;

public sealed class Buildplates
{
    [JsonInclude, JsonPropertyName("buildplates")]
    public readonly Dictionary<string, Buildplate> _buildplates = [];

    public Buildplates()
    {
        // empty
    }

    public void addBuildplate(string id, Buildplate buildplate)
        => _buildplates[id] = buildplate;

    public Buildplate? getBuildplate(string id)
        => _buildplates.GetOrDefault(id, null);

    public sealed record BuildplateEntry(
        string id,
        Buildplate buildplate
    );

    public BuildplateEntry[] getBuildplates()
        => [.. _buildplates.Select(entry => new BuildplateEntry(entry.Key, entry.Value))];

    public sealed class Buildplate
    {
        [JsonInclude]
        public readonly int size;
        [JsonInclude]
        public readonly int offset;
        [JsonInclude]
        public readonly int scale;

        [JsonInclude]
        public readonly bool night;

        [JsonInclude]
        public long lastModified;
        [JsonInclude]
        public string serverDataObjectId;
        [JsonInclude]
        public string previewObjectId;

        public Buildplate(int size, int offset, int scale, bool night, long lastModified, string serverDataObjectId, string previewObjectId)
        {
            this.size = size;
            this.offset = offset;
            this.scale = scale;

            this.night = night;

            this.lastModified = lastModified;
            this.serverDataObjectId = serverDataObjectId;
            this.previewObjectId = previewObjectId;
        }
    }
}
