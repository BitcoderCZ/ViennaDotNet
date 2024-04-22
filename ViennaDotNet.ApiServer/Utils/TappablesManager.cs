using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Serilog;
using System;
using System.IO;
using ViennaDotNet.Common;
using ViennaDotNet.Common.Utils;
using ViennaDotNet.EventBus.Client;
using static ViennaDotNet.ApiServer.Utils.TappablesManager.Tappable;

namespace ViennaDotNet.ApiServer.Utils
{
    public sealed class TappablesManager
    {
        private readonly Publisher publisher;
        private readonly Subscriber subscriber;

        private readonly Dictionary<string, Dictionary<string, Tappable>> tappables = new();

        public TappablesManager(EventBusClient eventBusClient)
        {
            publisher = eventBusClient.addPublisher();
            subscriber = eventBusClient.addSubscriber("tappables", new Subscriber.SubscriberListener(handleEvent, () =>
            {
                Log.Fatal("Tappables event bus subscriber error");
                Environment.Exit(1);
            }));
        }

        public Tappable[] getTappablesAround(double lat, double lon, double radius)
        {
            return getTileIdsAround(lat, lon, radius)
                .Select(tileId => tappables.GetOrDefault(tileId, null))
                .Where(tappables => tappables != null)
                .Select(items => items!.Values)
                .SelectMany(stream => stream) //.flatMap(Collection::stream)
                .Where(tappable =>
                {
                    double dx = lonToX(tappable.lon) * (1 << 16) - lonToX(lon) * (1 << 16);
                    double dy = latToY(tappable.lat) * (1 << 16) - latToY(lat) * (1 << 16);
                    double distanceSquared = dx * dx + dy * dy;
                    return distanceSquared <= radius * radius;
                })
                .ToArray();
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

        public void notifyTileActive(string playerId, double lat, double lon)
        {
            int tileX = xToTile(lonToX(lon));
            int tileY = yToTile(latToY(lat));
            publisher.publish("tappables", "activeTile", JsonConvert.SerializeObject(new ActiveTileNotification(tileX, tileY, playerId))).ContinueWith(task =>
            {
                if (!task.Result)
                    Log.Error("Event bus server rejected active tile notification event");
            });
        }

        private record ActiveTileNotification(
            int x,
            int y,
            string playerId
        )
        {
        }

        private void handleEvent(Subscriber.Event _event)
        {
            switch (_event.type)
            {
                case "tappableSpawn":
                    {
                        // TODO: prune expired tappables
                        Tappable? tappable;
                        try
                        {
                            tappable = JsonConvert.DeserializeObject<Tappable>(_event.data);
                            if (tappable is null)
                                throw new Exception("tappable is null");
                        }
                        catch (Exception exception)
                        {
                            Log.Error("Could not deserialise tappable spawn event", exception);
                            break;
                        }
                        addTappable(tappable);
                        break;
                    }
                case "activeTile":
                    break;
                default:
                    {
                        Log.Error($"Invalid tappables event bus event type {_event.type}");
                        break;
                    }
            }
        }

        private void addTappable(Tappable tappable)
        {
            string tileId = locationToTileId(tappable.lat, tappable.lon);
            tappables.ComputeIfAbsent(tileId, tileId1 => new())![tappable.id] = tappable;
        }

        public static string locationToTileId(float lat, float lon)
        {
            return $"{xToTile(lonToX(lon))}_{yToTile(latToY(lat))}";
        }

        private static double lonToX(double lon)
        {
            return ((1.0 + MathE.ToRadians(lon) / Math.PI) / 2.0);
        }

        private static double latToY(double lat)
        {
            return ((1.0 - (Math.Log(Math.Tan(MathE.ToRadians(lat)) + 1.0 / Math.Cos(MathE.ToRadians(lat)))) / Math.PI) / 2.0);
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
            Rarity rarity,
            Drops drops
        )
        {
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
    }
}
