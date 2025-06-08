using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using ViennaDotNet.ApiServer.Types.Common;

namespace ViennaDotNet.ApiServer.Types.Tappables;

public record ActiveLocation(
    string id,
    string tileId,
    Coordinate coordinate,
    string spawnTime,
    string expirationTime,
    ActiveLocation.Type type,
    string icon,
    ActiveLocation.Metadata metadata,
    ActiveLocation.TappableMetadata? tappableMetadata,
    ActiveLocation.EncounterMetadata? encounterMetadata
)
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Type
    {
        [EnumMember(Value = "Tappable")] TAPPABLE,
        [EnumMember(Value = "Encounter")] ENCOUNTER,
        [EnumMember(Value = "PlayerAdventure")] PLAYER_ADVENTURE,
    }

    public sealed record Metadata(
        string rewardId,
        Rarity rarity
    );

    public sealed record TappableMetadata(
        Rarity rarity
    );

    public sealed record EncounterMetadata(
        EncounterMetadata.EncounterType encounterType,
        string locationId,
        string worldId,
        EncounterMetadata.AnchorState anchorState,
        string anchorId,
        string augmentedImageSetId
    )
    {
        // TODO: what do these actually do?
        [JsonConverter(typeof(StringEnumConverter))]
        public enum EncounterType
        {
            [EnumMember(Value = "None")] NONE,
            [EnumMember(Value = "Short4X4Peaceful")] SHORT_4X4_PEACEFUL,
            [EnumMember(Value = "Short4X4Hostile")] SHORT_4X4_HOSTILE,
            [EnumMember(Value = "Short8X8Peaceful")] SHORT_8X8_PEACEFUL,
            [EnumMember(Value = "Short8X8Hostile")] SHORT_8X8_HOSTILE,
            [EnumMember(Value = "Short16X16Peaceful")] SHORT_16X16_PEACEFUL,
            [EnumMember(Value = "Short16X16Hostile")] SHORT_16X16_HOSTILE,
            [EnumMember(Value = "Tall4X4Peaceful")] TALL_4X4_PEACEFUL,
            [EnumMember(Value = "Tall4X4Hostile")] TALL_4X4_HOSTILE,
            [EnumMember(Value = "Tall8X8Peaceful")] TALL_8X8_PEACEFUL,
            [EnumMember(Value = "Tall8X8Hostile")] TALL_8X8_HOSTILE,
            [EnumMember(Value = "Tall16X16Peaceful")] TALL_16X16_PEACEFUL,
            [EnumMember(Value = "Tall16X16Hostile")] TALL_16X16_HOSTILE,
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum AnchorState
        {
            [EnumMember(Value = "Off")] OFF,
        }
    }
}
