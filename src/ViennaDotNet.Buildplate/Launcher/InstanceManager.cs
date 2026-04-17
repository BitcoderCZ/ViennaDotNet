using System.Text.Json.Serialization;
using Serilog;
using ViennaDotNet.Buildplate.Connector.Model;
using ViennaDotNet.Buildplate.Model;
using ViennaDotNet.Common;
using ViennaDotNet.Common.Utils;
using ViennaDotNet.EventBus.Client;

namespace ViennaDotNet.Buildplate.Launcher;

public sealed class InstanceManager
{
    private readonly Starter _starter;

    private readonly Publisher _publisher;
    private readonly RequestHandler _requestHandler;
    private int _runningInstanceCount = 0;
    private bool _shuttingDown = false;
    private readonly Lock _lock = new Lock();

    [JsonConverter(typeof(JsonStringEnumConverter))]
    private enum InstanceType
    {
        BUILD,
        PLAY,
        SHARED_BUILD,
        SHARED_PLAY,
        ENCOUNTER,
    }

    private sealed record StartRequest(
        string? PlayerId,
        string? EncounterId,
        string BuildplateId,
        bool Night,
        InstanceType Type,
        long ShutdownTime
    );

    private sealed record StartNotification(
        string InstanceId,
        string? PlayerId,
        string? EncounterId,
        string BuildplateId,
        string Address,
        int Port,
        InstanceType Type
    );

    public InstanceManager(EventBusClient eventBusClient, Starter starter)
    {
        _starter = starter;

        _publisher = eventBusClient.AddPublisher();

        _requestHandler = eventBusClient.AddRequestHandler("buildplates", new RequestHandler.Handler(
            async request =>
            {
                if (request.Type is "start")
                {
                    _lock.Enter();
                    if (_shuttingDown)
                    {
                        _lock.Exit();
                        return null;
                    }

                    _runningInstanceCount += 1;
                    _lock.Exit();

                    StartRequest startRequest;
                    try
                    {
                        startRequest = Json.Deserialize<StartRequest>(request.Data)!;
                    }
                    catch (Exception exception)
                    {
                        Log.Warning(exception, "Bad start request");
                        return null;
                    }

                    bool survival;
                    bool saveEnabled;
                    InventoryType inventoryType;
                    Instance.BuildplateSource buildplateSource;
                    long? shutdownTime;
                    switch (startRequest.Type)
                    {
                        case InstanceType.BUILD:
                            {
                                survival = false;
                                saveEnabled = true;
                                inventoryType = InventoryType.SYNCED;
                                buildplateSource = Instance.BuildplateSource.PLAYER;
                                shutdownTime = null;
                            }

                            break;
                        case InstanceType.PLAY:
                            {
                                survival = true;
                                saveEnabled = false;
                                inventoryType = InventoryType.DISCARD;
                                buildplateSource = Instance.BuildplateSource.PLAYER;
                                shutdownTime = null;
                            }

                            break;
                        case InstanceType.SHARED_BUILD:
                            {
                                survival = false;
                                saveEnabled = false;
                                inventoryType = InventoryType.DISCARD;
                                buildplateSource = Instance.BuildplateSource.SHARED;
                                shutdownTime = null;
                            }

                            break;
                        case InstanceType.SHARED_PLAY:
                            {
                                survival = true;
                                saveEnabled = false;
                                inventoryType = InventoryType.DISCARD;
                                buildplateSource = Instance.BuildplateSource.SHARED;
                                shutdownTime = null;
                            }

                            break;
                        case InstanceType.ENCOUNTER:
                            {
                                survival = true;
                                saveEnabled = false;
                                inventoryType = InventoryType.BACKPACK;
                                buildplateSource = Instance.BuildplateSource.ENCOUNTER;
                                shutdownTime = startRequest.ShutdownTime;
                            }

                            break;
                        default:
                            {
                                Log.Warning("Bad start request");
                                return null;
                            }
                    }

                    if (buildplateSource == Instance.BuildplateSource.PLAYER && startRequest.PlayerId is null)
                    {
                        Log.Warning("Bad start request");
                        return null;
                    }

                    string instanceId = U.RandomUuid().ToString();

                    Log.Information($"Starting buildplate instance {instanceId}");

                    Instance? instance = _starter.StartInstance(instanceId, startRequest.PlayerId, startRequest.BuildplateId, buildplateSource, survival, startRequest.Night, saveEnabled, inventoryType, shutdownTime);
                    if (instance is null)
                    {
                        Log.Error($"Error starting buildplate instance {instanceId}");
                        return null;
                    }

                    SendEventBusMessage("started", Json.Serialize(new StartNotification(
                        instanceId,
                        startRequest.PlayerId,
                        startRequest.EncounterId,
                        startRequest.BuildplateId,
                        instance.PublicAddress,
                        instance.Port,
                        startRequest.Type
                    )));

                    Task.Run(async () =>
                    {
                        try
                        {
                            await instance.WaitForShutdownAsync();

                            SendEventBusMessage("stopped", instance.InstanceId);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Failed to send stopped message");
                        }

                        _lock.Enter();
                        _runningInstanceCount -= 1;
                        _lock.Exit();
                    }).Forget();

                    return instanceId;
                }
                else if (request.Type is "preview")
                {
                    PreviewRequest previewRequest;
                    byte[] serverData;
                    try
                    {
                        previewRequest = Json.Deserialize<PreviewRequest>(request.Data)!;
                        serverData = Convert.FromBase64String(previewRequest.ServerDataBase64);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning($"Bad preview request: {ex}");
                        return null;
                    }

                    Log.Information("Generating buildplate preview");

                    string? preview = PreviewGenerator.GeneratePreview(serverData, previewRequest.Night, Program.StaticDataPath);
                    if (preview is null)
                    {
                        Log.Warning("Could not generate preview for buildplate");
                    }

                    return preview;
                }
                else
                {
                    return null;
                }
            },
            () =>
            {
                Log.Error("Event bus request handler error");
            }
        ));
    }

    private void SendEventBusMessage(string type, string message)
        => _publisher.Publish("buildplates", type, message).ContinueWith(task =>
        {
            if (!task.Result)
            {
                Log.Error("Event bus publisher error");
            }
        });

    public async Task Shutdown()
    {
        _requestHandler.Close();

        _lock.Enter();
        _shuttingDown = true;
        Log.Information($"Shutdown signal received, no new buildplate instances will be started, waiting for {_runningInstanceCount} instances to finish");
        while (_runningInstanceCount > 0)
        {
            int runningInstanceCount = _runningInstanceCount;
            _lock.Exit();

            await Task.Delay(1000);

            _lock.Enter();
            if (_runningInstanceCount != runningInstanceCount)
            {
                Log.Information($"Waiting for {runningInstanceCount} instances to finish");
            }
        }

        _lock.Exit();

        _publisher.Flush();
        _publisher.Close();
    }
}