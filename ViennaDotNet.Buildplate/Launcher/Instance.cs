using Cyotek.Data.Nbt;
using Cyotek.Data.Nbt.Serialization;
using Serilog;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Text.Json.Serialization;
using ViennaDotNet.Buildplate.Connector.Model;
using ViennaDotNet.Common;
using ViennaDotNet.Common.Buildplate.Connector.Model;
using ViennaDotNet.Common.Utils;
using ViennaDotNet.EventBus.Client;

namespace ViennaDotNet.Buildplate.Launcher;

public class Instance
{
    private const int HOST_PLAYER_CONNECT_TIMEOUT = 20000;

    public static Instance run(EventBusClient eventBusClient, string? playerId, string buildplateId, BuildplateSource buildplateSource, string instanceId, bool survival, bool night, bool saveEnabled, InventoryType inventoryType, long? shutdownTime, string publicAddress, int port, int serverInternalPort, string javaCmd, FileInfo fountainBridgeJar, DirectoryInfo serverTemplateDir, string fabricJarName, FileInfo connectorPluginJar, DirectoryInfo baseDir, string eventBusConnectionstring)
    {
        if (playerId is null && buildplateSource == BuildplateSource.PLAYER)
        {
            throw new ArgumentException($"{nameof(playerId)} was not while {nameof(buildplateSource)} was {nameof(BuildplateSource.PLAYER)}.", nameof(playerId));
        }

        Instance instance = new Instance(eventBusClient, playerId, buildplateId, buildplateSource, instanceId, survival, night, saveEnabled, inventoryType, shutdownTime, publicAddress, port, serverInternalPort, javaCmd, fountainBridgeJar, serverTemplateDir, fabricJarName, connectorPluginJar, baseDir, eventBusConnectionstring);

        new Thread(instance.run)
        {
            Name = $"Instance {instanceId}"
        }.Start();

        return instance;
    }

    private readonly EventBusClient eventBusClient;

    private readonly string? _playerId;
    private readonly string _buildplateId;
    private readonly BuildplateSource _buildplateSource;
    public readonly string InstanceId;
    private readonly bool _survival;
    private readonly bool _night;
    private readonly bool _saveEnabled;
    private readonly InventoryType _inventoryType;
    private readonly long? _shutdownTime;

    public readonly string PublicAddress;
    public readonly int Port;
    private readonly int _serverInternalPort;

    private readonly string _javaCmd;
    private readonly FileInfo _fountainBridgeJar;
    private readonly DirectoryInfo _serverTemplateDir;
    private readonly string _fabricJarName;
    private readonly FileInfo _connectorPluginJar;
    private readonly DirectoryInfo _baseDir;
    private readonly string _eventBusAddress;
    private readonly string _eventBusQueueName;
    private readonly string _connectorPluginArgString;

    private Thread? _thread;

    private Publisher? _publisher = null;
    private RequestSender? _requestSender = null;

    private Subscriber? _subscriber = null;
    private RequestHandler? _requestHandler = null;

    private DirectoryInfo? _serverWorkDir;
    private DirectoryInfo? _bridgeWorkDir;
    private ConsoleProcess? _serverProcess = null;
    private ConsoleProcess? _bridgeProcess = null;
    private bool _shuttingDown = false;
    private readonly object _subprocessLock = new();

    private volatile bool _hostPlayerConnected = false;

    private Instance(EventBusClient eventBusClient, string? playerId, string buildplateId, BuildplateSource buildplateSource, string instanceId, bool survival, bool night, bool saveEnabled, InventoryType inventoryType, long? shutdownTime, string publicAddress, int port, int serverInternalPort, string javaCmd, FileInfo fountainBridgeJar, DirectoryInfo serverTemplateDir, string fabricJarName, FileInfo connectorPluginJar, DirectoryInfo baseDir, string eventBusConnectionString)
    {
        this.eventBusClient = eventBusClient;

        _playerId = playerId;
        _buildplateId = buildplateId;
        _buildplateSource = buildplateSource;
        InstanceId = instanceId;
        _survival = survival;
        _night = night;
        _saveEnabled = saveEnabled;
        _inventoryType = inventoryType;
        _shutdownTime = shutdownTime;

        PublicAddress = publicAddress;
        Port = port;
        _serverInternalPort = serverInternalPort;

        _javaCmd = javaCmd;
        _fountainBridgeJar = fountainBridgeJar;
        _serverTemplateDir = serverTemplateDir;
        _fabricJarName = fabricJarName;
        _connectorPluginJar = connectorPluginJar;
        _baseDir = baseDir;
        _eventBusAddress = eventBusConnectionString;
        _eventBusQueueName = "buildplate_" + InstanceId;
        _connectorPluginArgString = Json.Serialize(new ConnectorPluginArg(_eventBusAddress, _eventBusQueueName, _inventoryType)).Replace("\"", "\\\"", StringComparison.Ordinal);
    }

    private void run()
    {
        _thread = Thread.CurrentThread;

        try
        {
            Log.Information(_buildplateSource switch
            {
                BuildplateSource.PLAYER => $"Starting for player {_playerId} buildplate {_buildplateId} (survival = {_survival}, saveEnabled = {_saveEnabled}, inventoryType = {_inventoryType})",
                BuildplateSource.SHARED => $"Starting for shared buildplate {_buildplateId} (player = {_playerId}, survival = {_survival}, saveEnabled = {_saveEnabled}, inventoryType = {_inventoryType})",
                BuildplateSource.ENCOUNTER => $"Starting for encounter buildplate {_buildplateId} (player = {_playerId}, survival = {_survival}, saveEnabled = {_saveEnabled}, inventoryType = {_inventoryType})",
                _ => throw new UnreachableException(),
            });
            Log.Information($"Using port {Port} internal port {_serverInternalPort}");

            _publisher = eventBusClient.addPublisher();
            _requestSender = eventBusClient.addRequestSender();

            Log.Information("Setting up server");

            BuildplateLoadResponse? buildplateLoadResponse = _buildplateSource switch
            {
                BuildplateSource.PLAYER => sendEventBusRequestRaw<BuildplateLoadResponse>("load", new BuildplateLoadRequest(_playerId!, _buildplateId), true).Result,
                BuildplateSource.SHARED => sendEventBusRequestRaw<BuildplateLoadResponse>("loadShared", new SharedBuildplateLoadRequest(_buildplateId), true).Result,
                BuildplateSource.ENCOUNTER => sendEventBusRequestRaw<BuildplateLoadResponse>("loadEncounter", new EncounterBuildplateLoadRequest(_buildplateId), true).Result,
                _ => throw new UnreachableException(),
            };

            Debug.Assert(buildplateLoadResponse is not null);

            byte[] serverData;
            try
            {
                serverData = Convert.FromBase64String(buildplateLoadResponse.serverDataBase64);
            }
            catch
            {
                Log.Error("Buildplate load response contained invalid base64 data");
                return;
            }

            try
            {
                _serverWorkDir = setupServerFiles(serverData);
                if (_serverWorkDir == null)
                {
                    Log.Error("Could not set up files for server");
                    return;
                }
            }
            catch (IOException ex)
            {
                Log.Error($"Could not set up files for server: {ex}");
                return;
            }

            try
            {
                _bridgeWorkDir = setupBridgeFiles(serverData);
                if (_bridgeWorkDir == null)
                {
                    Log.Error("Could not set up files for bridge");
                    return;
                }
            }
            catch (IOException ex)
            {
                Log.Error("Could not set up files for bridge", ex);
                return;
            }

            Log.Information("Running server");

            _subscriber = eventBusClient.addSubscriber(_eventBusQueueName, new Subscriber.SubscriberListener(
                async @event => await handleConnectorEvent(@event),
                () =>
                {
                    Log.Error("Event bus subscriber error");
                    beginShutdown();
                }
            ));
            _requestHandler = eventBusClient.addRequestHandler(_eventBusQueueName, new RequestHandler.Handler(
                request =>
                {
                    object? responseObject = handleConnectorRequest(request);
                    return Task.FromResult(responseObject is not null ? Json.Serialize(responseObject) : null);
                },
                () =>
                {
                    Log.Error("Event bus request handler error");
                    beginShutdown();
                }
            ));

            Monitor.Enter(_subprocessLock);
            if (!_shuttingDown)
            {
                startServerProcess();
                if (_serverProcess != null)
                {
                    Monitor.Exit(_subprocessLock);
                    int exitCode = waitForProcess(_serverProcess.Process);
                    Monitor.Enter(_subprocessLock);
                    _serverProcess = null;
                    if (!_shuttingDown)
                        Log.Warning($"Server process has unexpectedly terminated with exit code {exitCode}");
                    else
                        Log.Information($"Server has finished with exit code {exitCode}");

                    _shuttingDown = true;

                    if (_bridgeProcess != null)
                    {
                        Log.Information("Bridge is still running, shutting it down now");
                        _bridgeProcess.StopAndWait();
                        Monitor.Exit(_subprocessLock);
                        exitCode = waitForProcess(_bridgeProcess.Process);
                        Monitor.Enter(_subprocessLock);
                        _bridgeProcess = null;
                        Log.Information($"Bridge has finished with exit code {exitCode}");
                    }
                }
                else
                    Log.Information("Server failed to start");
            }

            Monitor.Exit(_subprocessLock);
        }
        catch (Exception ex)
        {
            Log.Error($"Unhandled exception: {ex}");
        }
        finally
        {
            _subscriber?.close();

            _requestHandler?.close();

            if (_publisher is not null)
            {
                _publisher.flush();
                _publisher.close();
            }

            if (_requestSender is not null)
            {
                _requestSender.flush();
                _requestSender.close();
            }

            cleanupBaseDir();

            Log.Information("Finished");
        }
    }

    private async Task handleConnectorEvent(Subscriber.Event @event)
    {
        switch (@event.type)
        {
            case "started":
                {
                    Log.Information("Server is ready");
                    startBridgeProcess();
                    await sendEventBusInstanceStatusNotification("ready");
                    if (_shutdownTime != null)
                    {
                        startShutdownTimer();
                    }
                    else
                    {
                        startHostPlayerConnectTimeout();
                    }
                }

                break;
            case "saved":
                {
                    if (_saveEnabled)
                    {
                        WorldSavedMessage? worldSavedMessage = readJson<WorldSavedMessage>(@event.data);
                        if (worldSavedMessage is not null)
                        {
                            if (_hostPlayerConnected)

                            {
                                Log.Information("Saving snapshot");
                                _ = sendEventBusRequest<object>("saved", worldSavedMessage, false);
                            }
                            else
                            {
                                Log.Information("Not saving snapshot because host player never connected");
                            }
                        }
                    }
                    else
                    {
                        Log.Information("Ignoring save data because saving is disabled");
                    }
                }

                break;

            case "inventoryAdd":
                {
                    InventoryAddItemMessage? inventoryAddItemMessage = readJson<InventoryAddItemMessage>(@event.data);
                    if (inventoryAddItemMessage != null)
                        await sendEventBusRequest<object>("inventoryAdd", inventoryAddItemMessage, false);
                }

                break;
            case "inventoryUpdateWear":
                {
                    InventoryUpdateItemWearMessage? inventoryUpdateItemWearMessage = readJson<InventoryUpdateItemWearMessage>(@event.data);
                    if (inventoryUpdateItemWearMessage != null)
                        await sendEventBusRequest<object>("inventoryUpdateWear", inventoryUpdateItemWearMessage, false);
                }

                break;
            case "inventorySetHotbar":
                {
                    InventorySetHotbarMessage? inventorySetHotbarMessage = readJson<InventorySetHotbarMessage>(@event.data);
                    if (inventorySetHotbarMessage != null)
                        await sendEventBusRequest<object>("inventorySetHotbar", inventorySetHotbarMessage, false);
                }

                break;
        }
    }

    private async Task<object?> handleConnectorRequest(RequestHandler.Request request)
    {
        switch (request.type)
        {
            case "playerConnected":
                {
                    PlayerConnectedRequest? playerConnectedRequest = readJson<PlayerConnectedRequest>(request.data);
                    if (playerConnectedRequest is not null)
                    {
                        if (_playerId is not null && !_hostPlayerConnected && playerConnectedRequest.uuid != _playerId)
                        {
                            Log.Information($"Rejecting player connection for player {playerConnectedRequest.uuid} because the host player must connect first");
                            return new PlayerConnectedResponse(false, null);
                        }

                        PlayerConnectedResponse? playerConnectedResponse = await sendEventBusRequest<PlayerConnectedResponse>("playerConnected", playerConnectedRequest, true);
                        if (playerConnectedResponse is not null)
                        {
                            Log.Information($"Player {playerConnectedRequest.uuid} has connected");

                            if (_playerId is not null && !_hostPlayerConnected && playerConnectedRequest.uuid == _playerId)
                            {
                                _hostPlayerConnected = true;
                            }

                            return playerConnectedResponse;
                        }
                    }
                }

                break;
            case "playerDisconnected":
                {
                    Log.Debug("Player dicconnecting...");
                    PlayerDisconnectedRequest? playerDisconnectedRequest = readJson<PlayerDisconnectedRequest>(request.data);
                    if (playerDisconnectedRequest is not null)
                    {
                        PlayerDisconnectedResponse? playerDisconnectedResponse = await sendEventBusRequest<PlayerDisconnectedResponse>("playerDisconnected", playerDisconnectedRequest, true);
                        if (playerDisconnectedResponse is not null)
                        {
                            Log.Information($"Player {playerDisconnectedRequest.playerId} has disconnected");

                            if (_shutdownTime is null && _playerId is not null && playerDisconnectedRequest.playerId == _playerId)
                            {
                                Log.Information("Host player has disconnected, beginning shutdown");
                                beginShutdown();
                            }

                            return playerDisconnectedResponse;
                        }
                    }
                }

                break;
            case "playerDead":
                {
                    string? playerId = readJson<string>(request.data);
                    if (playerId is not null)
                    {
                        bool? respawn = await sendEventBusRequest<bool?>("playerDead", playerId, true);
                        if (respawn is not null)
                        {
                            return respawn.Value;
                        }
                    }
                }

                break;
            case "getInventory":
                {
                    string? playerId = readJson<string>(request.data);
                    if (playerId is not null)
                    {
                        InventoryResponse? inventoryResponse = await sendEventBusRequest<InventoryResponse>("getInventory", playerId, true);

                        if (inventoryResponse is not null)
                            return inventoryResponse;
                    }
                }

                break;
            case "inventoryRemove":
                {
                    InventoryRemoveItemRequest? inventoryRemoveItemRequest = readJson<InventoryRemoveItemRequest>(request.data);
                    if (inventoryRemoveItemRequest is not null)
                    {
                        if (inventoryRemoveItemRequest.instanceId is not null)
                        {
                            bool? success = await sendEventBusRequest<bool>("inventoryRemove", inventoryRemoveItemRequest, true);

                            if (success is not null)
                                return success;
                        }
                        else
                        {
                            int? removedCount = await sendEventBusRequest<int>("inventoryRemove", inventoryRemoveItemRequest, true);
                            if (removedCount is not null)
                                return removedCount;
                        }
                    }
                }

                break;
            case "findPlayer":
                {
                    FindPlayerIdRequest? findPlayerIdRequest = readJson<FindPlayerIdRequest>(request.data);
                    if (findPlayerIdRequest is not null)
                    {
                        // TODO
                        return findPlayerIdRequest.minecraftName;
                    }
                }

                break;
            case "getInitialPlayerState":
                {
                    string? playerId = readJson<string>(request.data);
                    if (playerId != null)
                    {
                        InitialPlayerStateResponse? initialPlayerStateResponse = await sendEventBusRequest<InitialPlayerStateResponse>("getInitialPlayerState", playerId, true);
                        if (initialPlayerStateResponse != null)
                        {
                            return initialPlayerStateResponse;
                        }
                    }
                }

                break;
        }

        return null;
    }

    private T? readJson<T>(string str)
    {
        try
        {
            return Json.Deserialize<T>(str);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to decode event bus message JSON: {ex}");
            beginShutdown();
            return default;
        }
    }

    private sealed record RequestWithInstanceId(
        string instanceId,
        object request
    );

    private async Task sendEventBusInstanceStatusNotification(string status)
    {
        Debug.Assert(_publisher is not null);

        bool result = await _publisher.publish("buildplates", status, InstanceId);

        if (!result)
        {
            Log.Error("Event bus publisher error");
            beginShutdown();
        }
    }

    private Task<T?> sendEventBusRequest<T>(string type, object obj, bool returnResponse)
    {
        RequestWithInstanceId request = new RequestWithInstanceId(InstanceId, obj);

        return sendEventBusRequestRaw<T>(type, request, returnResponse);
    }

    private async Task<T?> sendEventBusRequestRaw<T>(string type, object obj, bool returnResponse)
    {
        try
        {
            string? response = await _requestSender!.request("buildplates", type, Json.Serialize(obj)).Task;

            if (response == null)
            {
                Log.Error("Event bus request failed (no response)");
                beginShutdown();
                return default;
            }

            return returnResponse
                ? Json.Deserialize<T>(response)
                : default;
        }
        catch (Exception ex)
        {
            Log.Error($"Event bus request failed: {ex}");
            beginShutdown();
            return default;
        }
    }

    private DirectoryInfo? setupServerFiles(byte[] serverData)
    {
        DirectoryInfo workDir = new DirectoryInfo(Path.Combine(_baseDir.FullName, "server"));
        if (!workDir.TryCreate())
        {
            Log.Error("Could not create server working directory");
            return null;
        }

        if (!copyServerFile(Path.Combine(_serverTemplateDir.FullName, _fabricJarName), Path.Combine(workDir.FullName, _fabricJarName), false))
        {

            Log.Error($"Fabric JAR {_fabricJarName} does not exist in server template directory");
            return null;
        }

        bool warnedMissingServerFiles = false;
        if (!copyServerFile(Path.Combine(Path.Combine(_serverTemplateDir.FullName, ".fabric"), "server"), Path.Combine(workDir.FullName, ".fabric/server"), true))
        {
            if (!warnedMissingServerFiles)
            {

                Log.Warning("Server files were not pre-downloaded in server template directory, it is recommended to pre-download all server files to improve instance start-up time and reduce network data usage");
                warnedMissingServerFiles = true;
            }
        }

        if (!copyServerFile(Path.Combine(_serverTemplateDir.FullName, "libraries"), Path.Combine(workDir.FullName, "libraries"), true))
        {
            if (!warnedMissingServerFiles)
            {
                Log.Warning("Server files were not pre-downloaded in server template directory, it is recommended to pre-download all server files to improve instance start-up time and reduce network data usage");
                warnedMissingServerFiles = true;
            }
        }

        if (!copyServerFile(Path.Combine(_serverTemplateDir.FullName, "versions"), Path.Combine(workDir.FullName, "versions"), true))
        {
            if (!warnedMissingServerFiles)
            {
                Log.Warning("Server files were not pre-downloaded in server template directory, it is recommended to pre-download all server files to improve instance start-up time and reduce network data usage");
                warnedMissingServerFiles = true;
            }
        }

        if (!copyServerFile(Path.Combine(_serverTemplateDir.FullName, "mods"), Path.Combine(workDir.FullName, "mods"), true))
        {
            Log.Error("Mods directory was not present in server template directory, the buildplate server instance will not function correctly without the Fountain and Vienna Fabric mods installed");
        }

        File.WriteAllText(Path.Combine(workDir.FullName, "eula.txt"), "eula=true");

        string serverProperties = new StringBuilder()
            .Append("online-mode=false\n")
            .Append("enforce-secure-profile=false\n")
            .Append("sync-chunk-writes=false\n")
            .Append("spawn-protection=0\n")
            .Append($"server-port={_serverInternalPort}\n")
            .Append($"gamemode={(_survival ? "survival" : "creative")}\n")
            .Append($"vienna-event-bus-address={_eventBusAddress}\n")
            .Append($"vienna-event-bus-queue-name={_eventBusQueueName}\n")
            .ToString();
        File.WriteAllText(Path.Combine(workDir.FullName, "server.properties"), serverProperties);

        DirectoryInfo worldDir = new DirectoryInfo(Path.Combine(workDir.FullName, "world"));
        if (!worldDir.TryCreate())
        {
            Log.Error("Could not create server world directory");
            return null;
        }

        DirectoryInfo worldEntitiesDir = new DirectoryInfo(Path.Combine(worldDir.FullName, "entities"));
        if (!worldEntitiesDir.TryCreate())
        {
            Log.Error("Could not create server world entities directory");
            return null;
        }

        DirectoryInfo worldRegionDir = new DirectoryInfo(Path.Combine(worldDir.FullName, "region"));
        if (!worldRegionDir.TryCreate())
        {
            Log.Error("Could not create server world regions directory");
            return null;
        }

        TagCompound levelDatTag = createLevelDat(_survival, _night);
        using (FileStream fs = new FileStream(Path.Combine(worldDir.FullName, "level.dat"), FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
        using (GZipStream gzs = new GZipStream(fs, CompressionLevel.Optimal))
        {
            BinaryTagWriter writer = new BinaryTagWriter(gzs);
            writer.WriteStartDocument();
            writer.WriteStartTag(null, TagType.Compound);
            writer.WriteTag(levelDatTag);
            writer.WriteEndTag();
            writer.WriteEndDocument();
        }
        //NBTIO.writeFile(levelDatTag, new File(worldDir, "level.dat"));

        using (MemoryStream byteArrayInputStream = new MemoryStream(serverData))
        using (ZipArchive zipInputStream = new ZipArchive(byteArrayInputStream))
        {
            foreach (ZipArchiveEntry entry in zipInputStream.Entries)
            {
                if (entry.IsDirectory()) continue;

                string path = Path.Combine(worldDir.FullName, entry.FullName);

                using (Stream zipStream = entry.Open())
                using (FileStream fs = File.OpenWrite(path))
                    zipStream.CopyTo(fs);
            }
        }

        return workDir;
    }

    private static bool copyServerFile(string src, string dst, bool directory)
    {
        if (directory)
        {
            if (!Directory.Exists(src))
                return false;
        }
        else if (!File.Exists(src))
            return false;

        if (directory)
        {
            Files.WalkFileTree(src, new FileVisitor(
                path =>
                {
                    string dstPath;
                    try

                    {
                        dstPath = Path.Combine(dst, Path.GetRelativePath(src, path));
                    }
                    catch (ArgumentException ex)
                    {
                        throw new IOException(null, ex);
                    }

                    Directory.CreateDirectory(dstPath);
                    return FileVisitResult.CONTINUE;
                },
                path =>
                {
                    string dstPath;
                    try
                    {
                        dstPath = Path.Combine(dst, Path.GetRelativePath(src, path));
                    }
                    catch (ArgumentException ex)
                    {
                        throw new IOException(null, ex);
                    }

                    File.Copy(path, dstPath);
                    return FileVisitResult.CONTINUE;
                },
                (path, ex) =>
                {
                    return ex is not null
                        ? throw ex
                        : FileVisitResult.CONTINUE;
                },
                (path, ex) =>
                {
                    return ex is not null
                        ? throw ex
                        : FileVisitResult.CONTINUE;
                }
            ));
        }
        else
            File.Copy(src, dst);
        return true;
    }

    private static TagCompound createLevelDat(bool survival, bool night)
    {
        TagCompound dataTag = new NbtBuilder.Compound()
            .put("GameType", survival ? 0 : 1)
            .put("Difficulty", 1)
            .put("DayTime", !night ? 6000 : 18000)
            .put("GameRules", new NbtBuilder.Compound()
                .put("doDaylightCycle", "false")
                .put("doWeatherCycle", "false")
                .put("doMobSpawning", "false")
                .put("fountain:doMobDespawn", "false")
                .put("keepInventory", "true")
            )
            .put("WorldGenSettings", new NbtBuilder.Compound()
                .put("seed", (long)0)    // TODO
                .put("generate_features", (byte)0)
                .put("dimensions", new NbtBuilder.Compound()
                    .put("minecraft:overworld", new NbtBuilder.Compound()
                        .put("type", "minecraft:overworld")
                        .put("generator", new NbtBuilder.Compound()
                            .put("type", "fountain:wrapper")
                            .put("buildplate", new NbtBuilder.Compound()
                                .put("ground_level", 63))
                            .put("inner", new NbtBuilder.Compound()
                                .put("type", "minecraft:noise")
                                .put("settings", "minecraft:overworld")
                                .put("biome_source", new NbtBuilder.Compound()
                                    .put("type", "minecraft:multi_noise")
                                    .put("preset", "minecraft:overworld")
                                )
                            )
                        )
                    )
                    .put("minecraft:the_nether", new NbtBuilder.Compound()
                        .put("type", "minecraft:the_nether")
                        .put("generator", new NbtBuilder.Compound()
                            .put("type", "fountain:wrapper")
                            .put("buildplate", new NbtBuilder.Compound()
                                .put("ground_level", 32))
                            .put("inner", new NbtBuilder.Compound()
                                .put("type", "minecraft:noise")
                                .put("settings", "minecraft:nether")
                                .put("biome_source", new NbtBuilder.Compound()
                                    .put("type", "minecraft:fixed")
                                    .put("biome", "minecraft:nether_wastes")
                                )
                            )
                        )
                    )
                )
            )
            .put("DataVersion", 3700)
            .put("version", 19133)
            .put("Version", new NbtBuilder.Compound()
                .put("Id", 3700)
                .put("Name", "1.20.4")
                .put("Series", "main")
                .put("Snapshot", (byte)0)
            )
            .put("initialized", (byte)1)
            .build("Data");

        return dataTag;
    }

    private DirectoryInfo? setupBridgeFiles(byte[] serverData)
    {
        DirectoryInfo workDir = new DirectoryInfo(Path.Combine(_baseDir.FullName, "bridge"));
        if (!workDir.TryCreate())
        {
            Log.Error("Could not create bridge working directory");
            return null;
        }

        // empty

        return workDir;
    }

    private void cleanupBaseDir()
    {
        Log.Information("Cleaning up runtime directory");

        try
        {
            Files.WalkFileTree(_baseDir.FullName, new FileVisitor(
                path =>
                {
                    return FileVisitResult.CONTINUE;
                },
                path =>
                {
                    File.Delete(path);
                    return FileVisitResult.CONTINUE;
                },
                (path, ex) =>
                {
                    return ex is not null
                        ? throw ex
                        : FileVisitResult.CONTINUE;
                },
                (path, ex) =>
                {
                    if (ex is not null)
                        throw ex;

                    Directory.Delete(path);
                    return FileVisitResult.CONTINUE;
                }
            ));
        }
        catch (IOException ex)
        {
            Log.Error($"Exception while cleaning up runtime directory: {ex}");
        }
    }

    private void startServerProcess()
    {
        Monitor.Enter(_subprocessLock);

        if (_shuttingDown)
        {
            Log.Debug("Already shutting down, not starting server process");
            Monitor.Exit(_subprocessLock);
            return;
        }

        if (_serverProcess != null)
        {
            Log.Debug("Server process has already been started");
            Monitor.Exit(_subprocessLock);
            return;
        }

        Log.Information("Starting server process");

        try
        {
            bool useShellExecute = true;

            _serverProcess = new ConsoleProcess(_javaCmd, useShellExecute, !useShellExecute);

            if (!useShellExecute)
            {
                _serverProcess.StandartTextReceived += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        Log.Debug($"[server] {e.Data}");
                    }
                };
                _serverProcess.ErrorTextReceived += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        Log.Error($"[server] {e.Data}");
                    }
                };
            }

            _serverProcess.ExecuteAsync(_serverWorkDir!.FullName, ["-jar", _fabricJarName, "-nogui"]);

            Log.Information($"Server process started, PID {_serverProcess.Id}");
        }
        catch (IOException ex)
        {
            Log.Error($"Could not start server process: {ex}");
        }

        Monitor.Exit(_subprocessLock);
    }

    private void startBridgeProcess()
    {
        Monitor.Enter(_subprocessLock);

        if (_shuttingDown)
        {
            Log.Debug("Already shutting down, not starting bridge process");
            Monitor.Exit(_subprocessLock);
            return;
        }

        if (_bridgeProcess != null)
        {
            Log.Debug("Bridge process has already been started");
            Monitor.Exit(_subprocessLock);
            return;
        }

        Log.Information("Starting bridge process");

        try
        {
            bool useShellExecute = false;

            _bridgeProcess = new ConsoleProcess(_javaCmd, useShellExecute, !useShellExecute);
            if (!useShellExecute)
            {
                _bridgeProcess.StandartTextReceived += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        Log.Debug($"[bridge] {e.Data}");
                    }
                };
                _bridgeProcess.ErrorTextReceived += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        Log.Error($"[bridge] {e.Data}");
                    }
                };
            }

            _bridgeProcess.ProcessExited += (sender, e) =>
            {
                Monitor.Enter(_subprocessLock);
                if (!_shuttingDown)
                {
                    Log.Warning($"Bridge process has unexpectedly terminated with exit code {_bridgeProcess.ExitCode}");
                    _bridgeProcess = null;
                    beginShutdown();
                }

                Monitor.Exit(_subprocessLock);
            };

            _bridgeProcess.ExecuteAsync(_bridgeWorkDir!.FullName,
            [
                "-jar", _fountainBridgeJar.FullName,
                "-port", Port.ToString(),
                "-serverAddress", "127.0.0.1",
                "-serverPort", _serverInternalPort.ToString(),
                "-connectorPluginJar", _connectorPluginJar.FullName,
                "-connectorPluginClass", "micheal65536.vienna.buildplate.connector.plugin.ViennaConnectorPlugin",
                "-connectorPluginArg", _connectorPluginArgString,
                "-useUUIDAsUsername",
            ]);

            Log.Information($"Bridge process started, PID {_bridgeProcess.Id}");
        }
        catch (IOException ex)
        {
            Log.Error($"Could not start bridge process: {ex}");
        }

        Monitor.Exit(_subprocessLock);
    }

#pragma warning disable IDE0022
    private void startHostPlayerConnectTimeout()
    {
        new Thread(() =>
        {
            try
            {
                Thread.Sleep(HOST_PLAYER_CONNECT_TIMEOUT);
            }
            catch (ThreadInterruptedException exception)
            {
                throw new InvalidOperationException(null, exception);
            }

            lock (_subprocessLock)
            {
                if (_shuttingDown)
                    return;
            }

            if (!_hostPlayerConnected)
            {
                Log.Information("Host player has not connected yet, shutting down");
                beginShutdown();
            }
        }).Start();
    }

    private void startShutdownTimer()
    {
        new Thread(() =>
        {
            if (_shutdownTime is { } shutdownTimeVal)
            {
                long currentTime = U.CurrentTimeMillis();
                while (currentTime < shutdownTimeVal)
                {
                    long duration = shutdownTimeVal - currentTime;
                    if (duration > 0)
                    {
                        Log.Information($"Server will shut down in {duration} milliseconds");

                        /*try
                        {*/
                        Debug.Assert((duration > 2000 ? (duration / 2) : duration) < int.MaxValue);
                        Thread.Sleep((int)(duration > 2000 ? (duration / 2) : duration));
                        /*}
                        catch (ThreadInterruptedException exception)
                        {
                            throw new AssertionError(exception);
                        }*/
                    }

                    currentTime = U.CurrentTimeMillis();
                }
            }

            Log.Information("Shutdown time has been reached, shutting down");
            beginShutdown();
        }).Start();
    }

    private void beginShutdown()
    {
        // a "bit" ugly
        ((Func<Task>)(async () =>
        {
            await Task.Yield();

            Monitor.Enter(_subprocessLock);

            if (_shuttingDown)
            {
                Log.Debug("Already shutting down, not beginning shutdown");
                Monitor.Exit(_subprocessLock);
                return;
            }

            _shuttingDown = true;

            Log.Information("Beginning shutdown");

            await sendEventBusInstanceStatusNotification("shuttingDown");

            if (_bridgeProcess != null)
            {
                Log.Information("Waiting for bridge to shut down");
                Monitor.Exit(_subprocessLock);
                _bridgeProcess.StopAndWait();
                int exitCode = _bridgeProcess.ExitCode;//waitForProcess(bridgeProcess.Process);
                Monitor.Enter(_subprocessLock);
                _bridgeProcess = null;
                Log.Information($"Bridge has finished with exit code {exitCode}");
            }

            if (_serverProcess != null)
            {
                Log.Information("Asking the server to shut down");
                _serverProcess.StopAndWait();
            }

            Monitor.Exit(_subprocessLock);
        }))().Forget(ex =>
        {

        });
    }
#pragma warning restore IDE0022

    private static int waitForProcess(Process process)
    {
        int exitCode;
        for (; ; )
        {
            try
            {
                process.WaitForExit();
                exitCode = process.ExitCode;
                break;
            }
            catch (ThreadInterruptedException)
            {
                continue;
            }
        }

        return exitCode;
    }

    public void waitForShutdown()
    {
        for (; ; )
        {
            try
            {
                if (_thread is null)
                {
                    Log.Debug("thread is null in waitForShutdown");
                    continue;
                }

                _thread.Join();
                break;
            }
            catch (ThreadInterruptedException)
            {
                continue;
            }
        }
    }

    private sealed record BuildplateLoadRequest(
        string playerId,
        string buildplateId
    );

    private sealed record EncounterBuildplateLoadRequest(
        string encounterBuildplateId
    );

    private sealed record SharedBuildplateLoadRequest(
        string sharedBuildplateId
    );

    private sealed record BuildplateLoadResponse(
        string serverDataBase64
    );

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum BuildplateSource
    {
        PLAYER,
        SHARED,
        ENCOUNTER
    }
}
