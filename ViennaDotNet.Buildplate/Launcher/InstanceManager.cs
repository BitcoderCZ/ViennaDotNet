using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using System.Diagnostics;
using ViennaDotNet.Buildplate.Connector.Model;
using ViennaDotNet.Common.Utils;
using ViennaDotNet.EventBus.Client;

namespace ViennaDotNet.Buildplate.Launcher;

public class InstanceManager
{
    private readonly Starter starter;
    private readonly PreviewGenerator previewGenerator;

    private readonly Publisher publisher;
    private readonly RequestHandler requestHandler;
    private int runningInstanceCount = 0;
    private bool shuttingDown = false;
    private readonly object _lock = new();

    [JsonConverter(typeof(StringEnumConverter))]
    private enum InstanceType
    {
        BUILD,
        PLAY,
        SHARED_BUILD,
        SHARED_PLAY,
        ENCOUNTER,
    }

    private sealed record StartRequest(
        string? playerId,
        string? encounterId,
        string buildplateId,
        bool night,
        InstanceType type,
        long shutdownTime
    );

    private sealed record StartNotification(
        string instanceId,
        string? playerId,
        string? encounterId,
        string buildplateId,
        string address,
        int port,
        InstanceType type
    );

    private sealed record PreviewRequest(
        string serverDataBase64,
        bool night
    );

    public InstanceManager(EventBusClient eventBusClient, Starter starter, PreviewGenerator previewGenerator)
    {
        this.starter = starter;
        this.previewGenerator = previewGenerator;

        publisher = eventBusClient.addPublisher();

        requestHandler = eventBusClient.addRequestHandler("buildplates", new RequestHandler.Handler(
           async request =>
            {
                if (request.type == "start")
                {
                    Monitor.Enter(_lock);
                    if (shuttingDown)
                    {
                        Monitor.Exit(_lock);
                        return null;
                    }

                    runningInstanceCount += 1;
                    Monitor.Exit(_lock);

                    StartRequest startRequest;
                    try
                    {
                        startRequest = JsonConvert.DeserializeObject<StartRequest>(request.data)!;
                    }
                    catch (Exception ex)
                    {
                        Log.Warning($"Bad start request: {ex}");
                        return null;
                    }

                    var (survival, saveEnabled, inventoryType, buildplateSource, shutdownTime) = startRequest.type switch
                    {
                        InstanceType.BUILD => (false, true, InventoryType.SYNCED, Instance.BuildplateSource.PLAYER, (long?)null),
                        InstanceType.PLAY => (true, false, InventoryType.DISCARD, Instance.BuildplateSource.PLAYER, null),
                        InstanceType.SHARED_BUILD => (false, false, InventoryType.DISCARD, Instance.BuildplateSource.SHARED, null),
                        InstanceType.SHARED_PLAY => (true, false, InventoryType.DISCARD, Instance.BuildplateSource.SHARED, null),
                        InstanceType.ENCOUNTER => (true, false, InventoryType.BACKPACK, Instance.BuildplateSource.ENCOUNTER, startRequest.shutdownTime),
                        _ => throw new UnreachableException(),
                    };

                    if (buildplateSource is Instance.BuildplateSource.PLAYER && startRequest.playerId is null)
                    {
                        Log.Warning("Bad start request");
                        return null;
                    }

                    string instanceId = U.RandomUuid().ToString();

                    Log.Information($"Starting buildplate instance {instanceId}");

                    Instance? instance = starter.startInstance(instanceId, startRequest.playerId, startRequest.buildplateId, buildplateSource, survival, startRequest.night, saveEnabled, inventoryType, shutdownTime);
                    if (instance == null)
                    {
                        Log.Error($"Error starting buildplate instance {instanceId}");
                        return null;
                    }

                    sendEventBusMessage("started", JsonConvert.SerializeObject(new StartNotification(
                        instanceId,
                        startRequest.playerId,
                        startRequest.encounterId,
                        startRequest.buildplateId,
                        instance.publicAddress,
                        instance.port,
                        startRequest.type
                    )));

                    new Thread(() =>
                    {
                        instance.waitForShutdown();

                        sendEventBusMessage("stopped", instance.instanceId);

                        Monitor.Enter(_lock);
                        runningInstanceCount -= 1;
                        Monitor.Exit(_lock);
                    }).Start();

                    return instanceId;
                }
                else if (request.type == "preview")
                {
                    PreviewRequest previewRequest;
                    byte[] serverData;
                    try
                    {
                        previewRequest = JsonConvert.DeserializeObject<PreviewRequest>(request.data)!;
                        serverData = Convert.FromBase64String(previewRequest.serverDataBase64);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning($"Bad preview request: {ex}");
                        return null;
                    }

                    Log.Information("Generating buildplate preview");

                    string? preview = previewGenerator.generatePreview(serverData, previewRequest.night);
                    if (preview == null)
                        Log.Warning("Could not generate preview for buildplate");

                    return preview;
                }
                else
                    return null;
            },
            () =>
            {
                Log.Error("Event bus request handler error");
            }
        ));
    }

    private void sendEventBusMessage(string type, string message)
    {
        publisher.publish("buildplates", type, message).ContinueWith(task =>
        {
            if (!task.Result)
                Log.Error("Event bus publisher error");
        });
    }

    public void shutdown()
    {
        requestHandler.close();

        Monitor.Enter(_lock);
        shuttingDown = true;
        Log.Information($"Shutdown signal received, no new buildplate instances will be started, waiting for {runningInstanceCount} instances to finish");
        while (runningInstanceCount > 0)
        {
            int runningInstanceCount = this.runningInstanceCount;
            Monitor.Exit(_lock);

            try
            {
                Thread.Sleep(1000);
            }
            catch (ThreadInterruptedException)
            {
                // empty
            }

            Monitor.Enter(_lock);
            if (this.runningInstanceCount != runningInstanceCount)
                Log.Information($"Waiting for {this.runningInstanceCount} instances to finish");
        }

        Monitor.Exit(_lock);

        publisher.flush();
        publisher.close();
    }
}
