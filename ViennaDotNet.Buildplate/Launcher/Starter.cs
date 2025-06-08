using Serilog;
using System.Runtime.InteropServices;
using ViennaDotNet.Buildplate.Connector.Model;
using ViennaDotNet.Common.Utils;
using ViennaDotNet.EventBus.Client;

namespace ViennaDotNet.Buildplate.Launcher;

public class Starter
{
    private readonly EventBusClient eventBusClient;

    private readonly string publicAddress;
    private readonly string javaCmd;
    private readonly DirectoryInfo tmpDir;
    private readonly string eventBusConnectionString;

    private readonly FileInfo fountainBridgeJar;
    private readonly DirectoryInfo serverTemplateDir;
    private readonly string fabricJarName;
    private readonly FileInfo connectorPluginJar;

    // TODO: const ?
    private static readonly int BASE_PORT = 19132;
    private static readonly int SERVER_INTERNAL_BASE_PORT = 25565;
    private readonly HashSet<int> portsInUse = [];
    private readonly HashSet<int> serverInternalPortsInUse = [];

    public Starter(EventBusClient eventBusClient, string eventBusConnectionString, string publicAddress, string javaCmd, string bridgeJar, string serverTemplateDir, string fabricJarName, string connectorPluginJar)
    {
        this.eventBusClient = eventBusClient;

        this.publicAddress = publicAddress;
        this.javaCmd = javaCmd;
        this.tmpDir = new DirectoryInfo(Path.GetTempPath());
        this.eventBusConnectionString = eventBusConnectionString;

        this.fountainBridgeJar = new FileInfo(bridgeJar);
        this.serverTemplateDir = new DirectoryInfo(serverTemplateDir);
        this.fabricJarName = fabricJarName;
        this.connectorPluginJar = new FileInfo(connectorPluginJar);
    }

    public Instance? startInstance(string instanceId, string? playerId, string buildplateId, Instance.BuildplateSource buildplateSource, bool survival, bool night, bool saveEnabled, InventoryType inventoryType, long? shutdownTime)
    {
        DirectoryInfo? baseDir = createInstanceBaseDir(instanceId);
        if (baseDir is null)
        {
            return null;
        }

        int port = findPort(portsInUse, BASE_PORT);
        int serverInternalPort = findPort(serverInternalPortsInUse, SERVER_INTERNAL_BASE_PORT);
        Instance instance = Instance.run(eventBusClient, playerId, buildplateId, buildplateSource, instanceId, survival, night, saveEnabled, inventoryType, shutdownTime, publicAddress, port, serverInternalPort, javaCmd, fountainBridgeJar, serverTemplateDir, fabricJarName, connectorPluginJar, baseDir, eventBusConnectionString);
        new Thread(() =>
        {
            instance.waitForShutdown();
            releasePort(portsInUse, port);
            releasePort(serverInternalPortsInUse, serverInternalPort);
        }).Start();
        return instance;
    }

    private static int findPort(HashSet<int> portsInUse, int basePort)
    {
        lock (portsInUse)
        {
            int port = basePort;
            while (portsInUse.Contains(port))
                port++;

            portsInUse.Add(port);
            return port;
        }
    }

    private static void releasePort(HashSet<int> portsInUse, int port)
    {
        lock (portsInUse)
        {
            if (!portsInUse.Remove(port))
                throw new InvalidOperationException();
        }
    }

    private DirectoryInfo? createInstanceBaseDir(string instanceId)
    {
        DirectoryInfo file = new DirectoryInfo(Path.Combine(tmpDir.FullName, $"vienna-buildplate-instance_{instanceId}"));
        if (!file.TryCreate())
        {
            Log.Error($"Error creating instance base directory for {instanceId}");
            return null;
        }

        Log.Debug($"Created instance base directory {file.FullName}");
        return file;
    }
}
