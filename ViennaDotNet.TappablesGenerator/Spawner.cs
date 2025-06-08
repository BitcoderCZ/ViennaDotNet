using Newtonsoft.Json;
using Serilog;
using System;
using ViennaDotNet.Common.Utils;
using ViennaDotNet.EventBus.Client;

namespace ViennaDotNet.TappablesGenerator;

public class Spawner
{
    private static readonly long SPAWN_INTERVAL = 15 * 1000;

    private readonly ActiveTiles activeTiles;
    private readonly TappableGenerator tappableGenerator;
	private readonly EncounterGenerator encounterGenerator;
    private readonly Publisher publisher;

    private readonly int maxTappableLifetimeIntervals;

    private long spawnCycleTime;
    private int spawnCycleIndex;
    private readonly Dictionary<int, int> lastSpawnCycleForTile = [];

    public Spawner(EventBusClient eventBusClient, ActiveTiles activeTiles, TappableGenerator tappableGenerator, EncounterGenerator encounterGenerator)
    {
        this.activeTiles = activeTiles;

        this.tappableGenerator = tappableGenerator;
        this.encounterGenerator = encounterGenerator;
        this.publisher = eventBusClient.addPublisher();

        this.maxTappableLifetimeIntervals = (int)(long.Max(this.tappableGenerator.getMaxTappableLifetime(), this.encounterGenerator.getMaxEncounterLifetime()) / SPAWN_INTERVAL + 1);

        this.spawnCycleTime = U.CurrentTimeMillis();
        this.spawnCycleIndex = maxTappableLifetimeIntervals;
    }

    public void run()
    {
        long nextTime = U.CurrentTimeMillis() + SPAWN_INTERVAL;
        for (; ; )
        {
            try
            {
                Thread.Sleep(Math.Max(0, (int)(nextTime - U.CurrentTimeMillis())));
            }
            catch (ThreadInterruptedException)
            {
                Log.Information("Spawn thread was interrupted, exiting");
                break;
            }

            nextTime += SPAWN_INTERVAL;

            doSpawnCycle();
        }
    }

    public void spawnTile(int tileX, int tileY)
    {
        Log.Information($"Spawning tappables for tile {tileX},{tileY}");

        long spawnCycleTime = this.spawnCycleTime;
        int spawnCycleIndex = this.spawnCycleIndex;

        while (spawnCycleTime < U.CurrentTimeMillis())
        {
            spawnCycleTime += SPAWN_INTERVAL;
            spawnCycleIndex++;
        }

        doSpawnCyclesForTile(tileX, tileY, spawnCycleTime, spawnCycleIndex);
    }

    private void doSpawnCycle()
    {
        ActiveTiles.ActiveTile[] activeTiles = this.activeTiles.getActiveTiles(spawnCycleTime);

        Log.Information($"Spawning tappables for {activeTiles.Length} tiles");

        while (spawnCycleTime < U.CurrentTimeMillis())
        {
            spawnCycleTime += SPAWN_INTERVAL;
            spawnCycleIndex++;
        }

        foreach (ActiveTiles.ActiveTile activeTile in activeTiles)
            doSpawnCyclesForTile(activeTile.tileX, activeTile.tileY, spawnCycleTime, spawnCycleIndex);
    }

    private void doSpawnCyclesForTile(int tileX, int tileY, long spawnCycleTime, int spawnCycleIndex)
    {
        int lastSpawnCycle = lastSpawnCycleForTile.GetOrDefault((tileX << 16) + tileY, 0);
        int cyclesToSpawn = Math.Min(spawnCycleIndex - lastSpawnCycle, maxTappableLifetimeIntervals);
        for (int index = 0; index < cyclesToSpawn; index++)
            spawnTappablesForTile(tileX, tileY, spawnCycleTime - SPAWN_INTERVAL * (cyclesToSpawn - index - 1));

        lastSpawnCycleForTile[(tileX << 16) + tileY] = spawnCycleIndex;
    }

    private void spawnTappablesForTile(int tileX, int tileY, long currentTime)
    {
        foreach (Tappable tappable in tappableGenerator.generateTappables(tileX, tileY, currentTime))
        {
            if (!publisher.publish("tappables", "tappableSpawn", JsonConvert.SerializeObject(tappable)).Result)
            {
                Log.Error("Event bus server rejected tappable spawn event");
            }
        }

        foreach (Encounter encounter in encounterGenerator.generateEncounters(tileX, tileY, currentTime))
        {
            if (!publisher.publish("tappables", "encounterSpawn", JsonConvert.SerializeObject(encounter)).Result)
            {
                Log.Error("Event bus server rejected encounter spawn event");
            }
        }
    }
}
