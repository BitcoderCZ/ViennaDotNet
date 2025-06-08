using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViennaDotNet.TappablesGenerator;

public sealed record Encounter(
    string id,
    float lat,
    float lon,
    long spawnTime,
    long validFor,
    string icon,
    Encounter.Rarity rarity,
    string encounterBuildplateId
)
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Rarity
    {
        COMMON,
        UNCOMMON,
        RARE,
        EPIC,
        LEGENDARY
    }
}