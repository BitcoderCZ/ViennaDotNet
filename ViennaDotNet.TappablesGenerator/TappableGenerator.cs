using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using System.Runtime.Serialization;
using ViennaDotNet.Common;
using ViennaDotNet.Common.Utils;

using Rarity = ViennaDotNet.TappablesGenerator.TappableGenerator.TappableConfig.Rarity;

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

public class TappableGenerator
{
    // TODO: make these configurable
    private static readonly int MIN_COUNT = 1;
    private static readonly int MAX_COUNT = 3;
    private static readonly long MIN_DURATION = 2 * 60 * 1000;
    private static readonly long MAX_DURATION = 5 * 60 * 1000;
    private static readonly long MIN_DELAY = 1 * 60 * 1000;
    private static readonly long MAX_DELAY = 2 * 60 * 1000;

    internal sealed record TappableConfig(
        string tappableID,
        Rarity rarity,
        int experiencePoints,
        string[][] possibleDropSets,
        Dictionary<string, TappableConfig.ItemCount> possibleItemCount
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
            [EnumMember(Value = "Legendary")] LEGENDARY
        }

        public sealed record ItemCount(
            int min,
            int max
        );
    }

    private readonly TappableConfig[] tappableConfigs;
    private readonly float totalWeight;

    private readonly Random random;

    public TappableGenerator()
    {
        try
        {
            Log.Information("Loading tappable generator data");
            string dataDir = Path.Combine("data", "tappable");
            LinkedList<TappableConfig> tappableConfigs = new();
            foreach (string file in Directory.EnumerateFiles(dataDir))
            {
                tappableConfigs.AddLast(JsonConvert.DeserializeObject<TappableConfig>(File.ReadAllText(file))!);
            }

            this.tappableConfigs = tappableConfigs.ToArray();
            totalWeight = tappableConfigs.Select(tappableConfig => tappableConfig.rarity.GetWeight()).Sum();
        }
        catch (Exception ex)
        {
            Log.Fatal($"Failed to load tappable generator data: {ex}");
            Environment.Exit(1);
            throw new InvalidOperationException();
        }

        Log.Information("Loaded tappable generator data");

        if (tappableConfigs.Length == 0)
        {
            Log.Fatal("No tappable configs provided");
            Environment.Exit(1);
            throw new InvalidOperationException();
        }

        foreach (TappableConfig tappableConfig in tappableConfigs)
        {
            if (tappableConfig.possibleDropSets.Length == 0)
                Log.Warning($"Tappable config {tappableConfig.tappableID} has no drop sets");

            foreach (string? itemId in tappableConfig.possibleDropSets
                 .SelectMany(a => a))
            {
                if (!tappableConfig.possibleItemCount.ContainsKey(itemId))
                {
                    Log.Fatal($"Tappable config {tappableConfig.tappableID} has no item count for item {itemId}");
                    Environment.Exit(1);
                    throw new InvalidOperationException();
                }
            }
        }

        random = new Random();
    }

    public long getMaxTappableLifetime()
    {
        return MAX_DELAY + MAX_DURATION + 30 * 1000;
    }

    public Tappable[] generateTappables(int tileX, int tileY, long currentTime)
    {
        LinkedList<Tappable> tappables = new();
        for (int count = random.Next(MIN_COUNT, MAX_COUNT + 1); count > 0; count--)
        {
            long spawnDelay = random.NextInt64(MIN_DELAY, MAX_DELAY + 1);
            long duration = random.NextInt64(MIN_DURATION, MAX_DURATION + 1);

            float configPos = random.NextSingle() * totalWeight;
            TappableConfig? tappableConfig = null;
            foreach (TappableConfig tappableConfig1 in tappableConfigs)
            {
                tappableConfig = tappableConfig1;
                configPos -= tappableConfig1.rarity.GetWeight();
                if (configPos <= 0.0f)
                    break;
            }

            if (tappableConfig == null)
                throw new InvalidOperationException();

            float[] tileBounds = getTileBounds(tileX, tileY);
            float lat = random.NextSingle(tileBounds[1], tileBounds[0]);
            float lon = random.NextSingle(tileBounds[2], tileBounds[3]);

            LinkedList<Tappable.Drops.Item> items = new();
            string[] dropSet = tappableConfig.possibleDropSets[random.Next(0, tappableConfig.possibleDropSets.Length)];

            foreach (string itemId in dropSet)
            {
                TappableConfig.ItemCount itemCount = tappableConfig.possibleItemCount[itemId];
                items.AddLast(new Tappable.Drops.Item(itemId, random.Next(itemCount.min, itemCount.max + 1)));
            }

            Tappable.Drops drops = new Tappable.Drops(
                tappableConfig.experiencePoints,
                items.ToArray()
            );

            Tappable tappable = new Tappable(
                U.RandomUuid().ToString(),
                lat,
                lon,
                currentTime + spawnDelay,
                duration,
                tappableConfig.tappableID,
                Enum.Parse<Tappable.Rarity>(tappableConfig.rarity.ToString()),
                drops
            );
            tappables.AddLast(tappable);
        }

        return tappables.ToArray();
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
        return (float)MathE.ToDegrees((x * 2.0 - 1.0) * Math.PI);
    }

    private static float yToLat(float y)
    {
        return (float)MathE.ToDegrees(Math.Atan(Math.Sinh((1.0 - y * 2.0) * Math.PI)));
    }
}