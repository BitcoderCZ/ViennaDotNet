using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace ViennaDotNet.ApiServer.Types.Tappables;


public sealed record EncounterState(
    EncounterState.ActiveEncounterState activeEncounterState
)
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ActiveEncounterState
    {
        [EnumMember(Value = "Pristine")] PRISTINE,
        [EnumMember(Value = "Dirty")] DIRTY,
    }
}