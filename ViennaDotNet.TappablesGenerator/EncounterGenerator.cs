using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Uma.Uuid;
using ViennaDotNet.Common.Utils;
using Rarity = ViennaDotNet.TappablesGenerator.EncounterGenerator.EncounterConfig.Rarity;

namespace ViennaDotNet.TappablesGenerator;

file static class RarityE
{
    private static readonly Dictionary<Rarity, float> valueToWeight = new Dictionary<Rarity, float>()
    {
        { Rarity.COMMON, 1.0f },
        { Rarity.UNCOMMON, 0.75f },
        { Rarity.RARE, 0.5f },
        { Rarity.EPIC, 0.25f },
        { Rarity.LEGENDARY, 0.125f },
    };

    public static float GetWeight(this Rarity rarity)
        => valueToWeight[rarity];
}

public class EncounterGenerator
{
    // TODO: make these configurable
    private static readonly int CHANCE_PER_TILE = 4;
    private static readonly long MIN_DELAY = 1 * 60 * 1000;
    private static readonly long MAX_DELAY = 2 * 60 * 1000;

    internal sealed record EncounterConfig(
        string icon,
        EncounterConfig.Rarity rarity,
        string encounterBuildplateId,
        int duration
    )
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Rarity
        {
            // TODO: find actual weights
            [EnumMember(Value = "Common")] COMMON,
            [EnumMember(Value = "Uncommon")] UNCOMMON,
            [EnumMember(Value = "Rare")] RARE,
            [EnumMember(Value = "Epic")] EPIC,
            [EnumMember(Value = "Legendary")] LEGENDARY,
        }
    }

    private readonly EncounterConfig[] encounterConfigs;
    private readonly float totalWeight;
    private readonly int maxDuration;

    private readonly Random random;

    public EncounterGenerator()
    {
        try
        {
            Log.Information("Loading encounter generator data");
            string dataDir = Path.Combine("data", "encounter");
            List<EncounterConfig> encounterConfigs = [];
            foreach (string file in Directory.EnumerateFiles(dataDir))
            {
                encounterConfigs.Add(JsonConvert.DeserializeObject<EncounterConfig>(File.ReadAllText(file))!);
            }

            this.encounterConfigs = [.. encounterConfigs];
            totalWeight = (float)encounterConfigs.Select(encounterConfig => (double)encounterConfig.rarity.GetWeight()).Sum();
            maxDuration = encounterConfigs.Select(encounterConfig => (int)encounterConfig.duration).Max() * 1000;
        }
        catch (Exception exception)
        {
            Log.Fatal("Failed to load encounter generator data", exception);
            Environment.Exit(1);
        }

        if (encounterConfigs.Length == 0)
        {
            Log.Warning("No encounter configs provided");
        }

        random = new Random();
    }

    public long getMaxEncounterLifetime()
    {
        return MAX_DELAY + this.maxDuration + 30 * 1000;
    }

    public Encounter[] generateEncounters(int tileX, int tileY, long currentTime)
    {
        if (encounterConfigs.Length == 0)
        {
            return [];
        }

        List<Encounter> encounters = [];
        if (random.Next(0, CHANCE_PER_TILE) == 0)
        {
            long spawnDelay = random.NextInt64(MIN_DELAY, MAX_DELAY + 1);

            float configPos = random.NextSingle() * totalWeight;
            EncounterConfig? encounterConfig = null;
            foreach (EncounterConfig encounterConfig1 in encounterConfigs)
            {
                encounterConfig = encounterConfig1;
                configPos -= encounterConfig1.rarity.GetWeight();
                if (configPos <= 0.0f)
                {
                    break;
                }
            }

            if (encounterConfig is null)
            {
                throw new UnreachableException();
            }

            float[] tileBounds = getTileBounds(tileX, tileY);
            float lat = random.NextSingle(tileBounds[1], tileBounds[0]);
            float lon = random.NextSingle(tileBounds[2], tileBounds[3]);

            Encounter encounter = new Encounter(
                    U.RandomUuid().ToString(),
                    lat,
                    lon,
                    currentTime + spawnDelay,
                    encounterConfig.duration * 1000,
                    encounterConfig.icon,
                    Enum.Parse<Encounter.Rarity>(encounterConfig.rarity.ToString()),
                    encounterConfig.encounterBuildplateId
            );
            encounters.Add(encounter);
        }

        return [.. encounters];
    }

    private static float[] getTileBounds(int tileX, int tileY)
    {
        return [
                yToLat((float) tileY / (1 << 16)),
                yToLat((float) (tileY + 1) / (1 << 16)),
                xToLon((float) tileX / (1 << 16)),
                xToLon((float) (tileX + 1) / (1 << 16))
        ];
    }

    private static float xToLon(float x)
    {
        return ((x * 2.0f - 1.0f) * float.Pi) * (180f / float.Pi);
    }

    private static float yToLat(float y)
    {
        return (float.Atan(float.Sinh((1.0f - y * 2.0f) * float.Pi))) * (180f / float.Pi);
    }
}