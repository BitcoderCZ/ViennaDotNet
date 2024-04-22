using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViennaDotNet.Common.Utils;
using ViennaDotNet.EventBus.Client;
using ViennaDotNet.TappablesGenerator;
using static ViennaDotNet.TappablesGenerator.ActiveTiles;

namespace ViennaDotNet.TappablesGenerator
{
    public class ActiveTiles
    {
        private static readonly int ACTIVE_TILE_RADIUS = 3;
        private static readonly long ACTIVE_TILE_EXPIRY_TIME = 2 * 60 * 1000;

        private readonly Dictionary<int, ActiveTile> activeTiles = new();

        public ActiveTiles(EventBusClient eventBusClient)
        {
            eventBusClient.addSubscriber("tappables", new Subscriber.SubscriberListener(_event =>
            {
                if (_event.type == "activeTile")
                {
                    ActiveTileNotification activeTileNotification;
                    try
                    {
                        activeTileNotification = JsonConvert.DeserializeObject<ActiveTileNotification>(_event.data)!;
                    }
                    catch (Exception exception)
                    {
                        Log.Error($"Could not deserialise active tile notification event: {exception}");
                        return;
                    }

                    long currentTime = U.CurrentTimeMillis();
                    this.pruneActiveTiles(currentTime);
                    for (int tileX = activeTileNotification.x - ACTIVE_TILE_RADIUS; tileX < activeTileNotification.x + ACTIVE_TILE_RADIUS + 1; tileX++)
                    {
                        for (int tileY = activeTileNotification.y - ACTIVE_TILE_RADIUS; tileY < activeTileNotification.y + ACTIVE_TILE_RADIUS + 1; tileY++)
                        {
                            this.markTileActive(tileX, tileY, currentTime);
                        }
                    }
                }
            }, () =>
            {
                Log.Error("Event bus subscriber error");
                Environment.Exit(1);
            }));
        }

        public ActiveTile[] getActiveTiles(long currentTime)
        {
            return activeTiles.Values.Where(activeTile => currentTime < activeTile.latestActiveTime + ACTIVE_TILE_EXPIRY_TIME).ToArray();
        }

        private void markTileActive(int tileX, int tileY, long currentTime)
        {
            ActiveTile? activeTile = activeTiles.GetOrDefault((tileX << 16) + tileY, null);
            if (activeTile == null)
            {
                Log.Information($"Tile {tileX},{tileY} is becoming active");
                activeTile = new ActiveTile(tileX, tileY, currentTime, currentTime);
            }
            else
                activeTile = new ActiveTile(tileX, tileY, activeTile.firstActiveTime, currentTime);

            activeTiles[(tileX << 16) + tileY] = activeTile;
        }

        private void pruneActiveTiles(long currentTime)
        {
            List<int> entriesToRemove = new();

            foreach (var entry in activeTiles)
            {
                if (entry.Value.latestActiveTime + ACTIVE_TILE_EXPIRY_TIME <= currentTime)
                {
                    Log.Information($"Tile {entry.Value.tileX},{entry.Value.tileY} is inactive");
                    entriesToRemove.Add(entry.Key);
                }
            }

            foreach (var key in entriesToRemove)
                activeTiles.Remove(key);
        }

        public record ActiveTile(
            int tileX,
            int tileY,
            long firstActiveTime,
            long latestActiveTime
        )
        {
        }

        record ActiveTileNotification(
            int x,
            int y,
            string playerId
        )
        {
        }
    }
}
