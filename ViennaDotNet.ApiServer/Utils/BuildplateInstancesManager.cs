using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using ViennaDotNet.Common.Utils;
using ViennaDotNet.EventBus.Client;

namespace ViennaDotNet.ApiServer.Utils;

public sealed class BuildplateInstancesManager
{
    private readonly EventBusClient eventBusClient;
    private readonly Subscriber subscriber;
    private readonly RequestSender requestSender;

    private readonly Dictionary<string, TaskCompletionSource<bool>?> pendingInstances = [];
    private readonly Dictionary<string, InstanceInfo> instances = [];
    private readonly Dictionary<string, HashSet<string>> instancesByBuildplateId = [];

    public BuildplateInstancesManager(EventBusClient eventBusClient)
    {
        this.eventBusClient = eventBusClient;
        subscriber = eventBusClient.addSubscriber("buildplates", new Subscriber.SubscriberListener(
            handleEvent,
            () =>
            {
                Log.Fatal("Buildplates event bus subscriber error");
                Log.CloseAndFlush();
                Environment.Exit(1);
            }
        ));
        requestSender = eventBusClient.addRequestSender();
    }

    public async Task<string?> requestBuildplateInstance(string? playerId, string? encounterId, string buildplateId, InstanceType type, long shutdownTime, bool night)
    {
        if (playerId == null && type != InstanceType.ENCOUNTER)
        {
            throw new ArgumentException();
        }

        if (encounterId != null && type != InstanceType.ENCOUNTER)
        {
            throw new ArgumentException();
        }

        if (playerId != null && encounterId != null)
        {
            Log.Information($"Finding buildplate instance for buildplate {buildplateId} type {type} encounter {encounterId} player {playerId}");
        }
        else if (playerId != null)
        {
            Log.Information($"Finding buildplate instance for buildplate {buildplateId} type {type} player {playerId}");
        }
        else if (encounterId != null)
        {
            Log.Information($"Finding buildplate instance for buildplate {buildplateId} type {type} encounter {encounterId}");
        }
        else
        {
            Log.Information($"Finding buildplate instance for buildplate {buildplateId} type {type}");
        }

        lock (instances)
        {
            HashSet<string>? instanceIds = instancesByBuildplateId.GetOrDefault(buildplateId);
            if (instanceIds is not null)
            {
                foreach (string loopInstanceId in instanceIds)
                {
                    InstanceInfo? instanceInfo = instances.GetOrDefault(loopInstanceId);
                    if (instanceInfo is not null)
                    {
                        if (instanceInfo.type == type &&
                            instanceInfo.playerId == playerId &&
                            instanceInfo.encounterId == encounterId
                        )
                        {
                            Log.Information($"Found existing buildplate instance {loopInstanceId}");
                            return loopInstanceId;
                        }
                    }
                }
            }
        }

        Log.Information("Did not find existing instance, starting new instance");
        string? instanceId = await requestSender.request("buildplates", "start", JsonConvert.SerializeObject(new StartRequest(playerId, encounterId, buildplateId, night, type, shutdownTime))).Task;
        if (instanceId == null)
        {
            Log.Error("Buildplate start request was rejected/ignored");
            return null;
        }

        TaskCompletionSource<bool> completableFuture = new();
        lock (instances)
        {
            if (instances.ContainsKey(instanceId))
                completableFuture.SetResult(true);
            else
                lock (pendingInstances)
                {
                    pendingInstances[instanceId] = completableFuture;
                }
        }

        if (!await completableFuture.Task)
        {
            Log.Warning($"Could not start buildplate instance {instanceId}");
            return null;
        }

        return instanceId;
    }

    public InstanceInfo? getInstanceInfo(string instanceId)
    {
        lock (instances)
        {
            return instances.GetOrDefault(instanceId, null);
        }
    }

    public string? getBuildplatePreview(byte[] serverData, bool night)
    {
        Log.Information("Requesting buildplate preview");

        string? preview = requestSender.request("buildplates", "preview", JsonConvert.SerializeObject(new PreviewRequest(Convert.ToBase64String(serverData), night))).Task.Result;
        if (preview == null)
            Log.Error("Preview request was rejected/ignored");

        return preview;
    }

    private void handleEvent(Subscriber.Event @event)
    {
        switch (@event.type)
        {
            case "started":
                {
                    StartNotification startNotification;
                    try
                    {
                        startNotification = JsonConvert.DeserializeObject<StartNotification>(@event.data)!;
                        if (startNotification.playerId == null && startNotification.type != InstanceType.ENCOUNTER)
                        {
                            Log.Warning("Bad start notification");
                            return;
                        }

                        lock (instances)
                        {
                            Log.Information($"Buildplate instance {startNotification.instanceId} has started");
                            instances[startNotification.instanceId] = new InstanceInfo(
                                startNotification.type,
                                startNotification.instanceId,
                                startNotification.playerId,
                                startNotification.encounterId,
                                startNotification.buildplateId,
                                startNotification.address,
                                startNotification.port,
                                false
                            );

                            instancesByBuildplateId.ComputeIfAbsent(startNotification.buildplateId, buildplateId => [])!.Add(startNotification.instanceId);
                        }

                        lock (pendingInstances)
                        {
                            TaskCompletionSource<bool>? completableFuture = pendingInstances.JavaRemove(startNotification.instanceId);
                            completableFuture?.SetResult(true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning($"Bad start notification: {ex}");
                    }
                }

                break;
            case "ready":
                {
                    string instanceId = @event.data;
                    lock (instances)
                    {
                        InstanceInfo? instanceInfo = instances.GetOrDefault(instanceId, null);
                        if (instanceInfo != null)
                        {
                            Log.Information($"Buildplate instance {instanceId} is ready");
                            instances[instanceId] = new InstanceInfo(
                                instanceInfo.type,
                                instanceInfo.instanceId,
                                instanceInfo.playerId,
                                instanceInfo.encounterId,
                                instanceInfo.buildplateId,
                                instanceInfo.address,
                                instanceInfo.port,
                                true
                            );
                        }
                    }
                }

                break;
            case "stopped":
                {
                    string instanceId = @event.data;
                    lock (instances)
                    {
                        var instanceInfo = instances.JavaRemove(instanceId);
                        if (instanceInfo != null)
                        {
                            Log.Information($"Buildplate instance {instanceId} has stopped");

                            var instanceIds = instancesByBuildplateId.GetOrDefault(instanceInfo.buildplateId);
                            instanceIds?.Remove(instanceInfo.instanceId);
                        }
                    }
                }

                break;
        }
    }

    private sealed record StartRequest(
        string? playerId,
        string? encounterId,
        string buildplateId,
        bool night,
        InstanceType type,
        long shutdownTime
    );

    private sealed record PreviewRequest(
        string serverDataBase64,
        bool night
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

    [JsonConverter(typeof(StringEnumConverter))]
    public enum InstanceType
    {
        BUILD,
        PLAY,
        SHARED_BUILD,
        SHARED_PLAY,
        ENCOUNTER,
    }

    public sealed record InstanceInfo(
        InstanceType type,

        string instanceId,

        string? playerId,
        string? encounterId,
        string buildplateId,

        string address,
        int port,

        bool ready
    );
}
