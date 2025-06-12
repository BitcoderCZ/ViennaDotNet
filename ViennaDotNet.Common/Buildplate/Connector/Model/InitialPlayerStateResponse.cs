using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViennaDotNet.Common.Buildplate.Connector.Model;

public sealed record InitialPlayerStateResponse(
    float health,
    InitialPlayerStateResponse.BoostStatusEffect[] boostStatusEffects
)
{
    public sealed record BoostStatusEffect(
        BoostStatusEffect.Type type,
        int value,
        long remainingDuration
    )
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Type
        {
            ADVENTURE_XP,
            DEFENSE,
            EATING,
            HEALTH,
            MINING_SPEED,
            STRENGTH
        }
    }
}