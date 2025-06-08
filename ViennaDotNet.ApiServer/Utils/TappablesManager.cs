using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using System.Diagnostics.Metrics;
using ViennaDotNet.Common;
using ViennaDotNet.Common.Utils;
using ViennaDotNet.EventBus.Client;
using static ViennaDotNet.ApiServer.Utils.TappablesManager;
using static ViennaDotNet.DB.Models.Player.ActivityLog;

namespace ViennaDotNet.ApiServer.Utils;

public sealed class TappablesManager
{
    private readonly Subscriber subscriber;
    private readonly RequestSender requestSender;

    private readonly Dictionary<string, Dictionary<string, Tappable>> tappables = [];
    private readonly Dictionary<string, Dictionary<string, Encounter>> encounters = [];
    private int pruneCounter = 0;

    public TappablesManager(EventBusClient eventBusClient)
    {
        subscriber = eventBusClient.addSubscriber("tappables", new Subscriber.SubscriberListener(handleEvent, () =>
        {
            Log.Fatal("Tappables event bus subscriber error");
            Environment.Exit(1);
        }));
        this.requestSender = eventBusClient.addRequestSender();
    }

    public Tappable[] getTappablesAround(double lat, double lon, double radius)
    {
        return [.. getTileIdsAround(lat, lon, radius)
            .Select(tileId => tappables.GetOrDefault(tileId, null))
            .Where(tappables => tappables != null)
            .Select(items => items!.Values)
            .SelectMany(stream => stream)
            .Where(tappable =>
            {
                double dx = lonToX(tappable.lon) * (1 << 16) - lonToX(lon) * (1 << 16);
                double dy = latToY(tappable.lat) * (1 << 16) - latToY(lat) * (1 << 16);
                double distanceSquared = dx * dx + dy * dy;
                return distanceSquared <= radius * radius;
            })];
    }

    public Encounter[] getEncountersAround(double lat, double lon, double radius)
    {
        return [.. getTileIdsAround(lat, lon, radius)
            .Select(tileId => encounters.GetOrDefault(tileId))
            .Where(encounters => encounters is not null)
            .SelectMany(encounters => encounters!.Values)
            .Where(encounter =>
            {
                double dx = lonToX(encounter.lon) * (1 << 16) - lonToX(lon) * (1 << 16);
                double dy = latToY(encounter.lat) * (1 << 16) - latToY(lat) * (1 << 16);
                double distanceSquared = dx * dx + dy * dy;
                return distanceSquared <= radius * radius;
            })];
    }

    private static string[] getTileIdsAround(double lat, double lon, double radius)
    {
        int tileX = xToTile(lonToX(lon));
        int tileY = yToTile(latToY(lat));
        int tileRadius = (int)Math.Ceiling(radius);
        return Java.IntStream.Range(tileX - tileRadius, tileX + tileRadius + 1).Select(x => Java.IntStream.Range(tileY - tileRadius, tileY + tileRadius + 1).Select(y => $"{x}_{y}")).SelectMany(stream => stream).ToArray();
    }

    public Tappable? getTappableWithId(string id, string tileId)
    {
        Dictionary<string, Tappable>? tappablesInTile = tappables.GetOrDefault(tileId, null);
        if (tappablesInTile != null)
        {
            Tappable? tappable = tappablesInTile.GetOrDefault(id, null);
            if (tappable != null)
                return tappable;
        }

        return null;
    }

    public Encounter? getEncounterWithId(string id, string tileId)
    {
        var encountersInTile = encounters.GetOrDefault(tileId);
        if (encountersInTile is not null)
        {
            var encounter = encountersInTile.GetOrDefault(id);
            if (encounter is not null)
            {
                return encounter;
            }
        }

        return null;
    }

    public void notifyTileActive(string playerId, double lat, double lon)
    {
        int tileX = xToTile(lonToX(lon));
        int tileY = yToTile(latToY(lat));
        string? response = requestSender.request("tappables", "activeTile", JsonConvert.SerializeObject(new ActiveTileNotification(tileX, tileY, playerId))).Task.Result;
        if (response == null)
            Log.Warning("Active tile notification event was rejected/ignored");
    }

    private sealed record ActiveTileNotification(
        int x,
        int y,
        string playerId
    );

    private void handleEvent(Subscriber.Event @event)
    {
        switch (@event.type)
        {
            case "tappableSpawn":
                {
                    Tappable? tappable;
                    try
                    {
                        tappable = JsonConvert.DeserializeObject<Tappable>(@event.data);
                        if (tappable is null)
                            throw new Exception("tappable is null");
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Could not deserialise tappable spawn event", ex);
                        break;
                    }

                    addTappable(tappable);
                    if (pruneCounter++ == 10)
                    {
                        pruneCounter = 0;
                        prune(@event.timestamp);
                    }

                    break;
                }
        }
    }

    private void addTappable(Tappable tappable)
    {
        string tileId = locationToTileId(tappable.lat, tappable.lon);
        tappables.ComputeIfAbsent(tileId, tileId1 => [])![tappable.id] = tappable;
    }

    private void addEncounter(Encounter encounter)
    {
        string tileId = locationToTileId(encounter.lat, encounter.lon);
        encounters.ComputeIfAbsent(tileId, tileId1 => [])![encounter.id] = encounter;
    }

    private void prune(long currentTime)
    {
        foreach (var tileTappables in tappables.Values)
        {
            tileTappables.RemoveIf(entry =>
            {
                Tappable tappable = entry.Value;
                long expiresAt = tappable.spawnTime + tappable.validFor;
                return expiresAt <= currentTime;
            });
        }

        tappables.RemoveIf(entry => entry.Value.IsEmpty());

        foreach (var tileEncounters in encounters.Values)
        {
            tileEncounters.RemoveIf(entry =>
            {
                Encounter encounter = entry.Value;
                long expiresAt = encounter.spawnTime + encounter.validFor;
                return expiresAt <= currentTime;
            });
        }

        encounters.RemoveIf(entry => entry.Value.Count == 0);
    }

    public static string locationToTileId(float lat, float lon)
    {
        return $"{xToTile(lonToX(lon))}_{yToTile(latToY(lat))}";
    }

    private static double lonToX(double lon)
    {
        return (1.0 + MathE.ToRadians(lon) / Math.PI) / 2.0;
    }

    private static double latToY(double lat)
    {
        return (1.0 - Math.Log(Math.Tan(MathE.ToRadians(lat)) + 1.0 / Math.Cos(MathE.ToRadians(lat))) / Math.PI) / 2.0;
    }

    private static int xToTile(double x)
    {
        return (int)Math.Floor(x * (1 << 16));
    }

    private static int yToTile(double y)
    {
        return (int)Math.Floor(y * (1 << 16));
    }

    public record Tappable(
        string id,
        float lat,
        float lon,
        long spawnTime,
        long validFor,
        string icon,
        Tappable.Rarity rarity,
        Tappable.Drops drops
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

        public record Drops(
            int experiencePoints,
            Drops.Item[] items
        )
        {
            public record Item(
                string id,
                int count
            )
            {
            }
        }
    }

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
}
