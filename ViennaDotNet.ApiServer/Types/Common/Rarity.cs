using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace ViennaDotNet.ApiServer.Types.Common
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Rarity
    {
        [EnumMember(Value = "Common")] COMMON,
        [EnumMember(Value = "Uncommon")] UNCOMMON,
        [EnumMember(Value = "Rare")] RARE,
        [EnumMember(Value = "Epic")] EPIC,
        [EnumMember(Value = "Legendary")] LEGENDARY,
        [EnumMember(Value = "oobe")] OOBE
    }
}
