using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net;
using Uma.Uuid;
using ViennaDotNet.Common.Utils;
using ViennaDotNet.EventBus.Client;

namespace ViennaDotNet.ApiServer.Utils
{
    public sealed class BuildplateInstancesManager
    {
        private readonly EventBusClient eventBusClient;
        private readonly Subscriber subscriber;
        private readonly RequestSender requestSender;

        private readonly Dictionary<string, TaskCompletionSource<bool>?> pendingInstances = new();
        private readonly Dictionary<string, InstanceInfo> instances = new();

        public BuildplateInstancesManager(EventBusClient eventBusClient)
        {
            this.eventBusClient = eventBusClient;
            this.subscriber = eventBusClient.addSubscriber("buildplates", new Subscriber.SubscriberListener(
                _event => handleEvent(_event),
                () =>
                {
                    Log.Fatal("Buildplates event bus subscriber error");
                    Environment.Exit(1);
                }
            ));
            this.requestSender = eventBusClient.addRequestSender();
        }

        public async Task<string?> startBuildplateInstance(string playerId, string buildplateId, bool night)
        {
            Log.Information($"Requesting buildplate instance for player {playerId} buildplate {buildplateId}");

            string? instanceId = requestSender.request("buildplates", "start", JsonConvert.SerializeObject(new StartRequest(playerId, buildplateId, false, night))).Task.Result;
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
                {
                    lock (pendingInstances)
                    {
                        pendingInstances[instanceId] = completableFuture;
                    }
                }
            }

            if (!(await completableFuture.Task))
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

                            lock (instances)
                            {
                                Log.Information($"Buildplate instance {startNotification.instanceId} has started");
                                instances[startNotification.instanceId] = new InstanceInfo(
                                    startNotification.instanceId,
                                    startNotification.playerId,
                                    startNotification.buildplateId,
                                    startNotification.address,
                                    startNotification.port,
                                    false
                                );
                            }

                            lock (pendingInstances)
                            {
                                TaskCompletionSource<bool>? completableFuture = pendingInstances.JavaRemove(startNotification.instanceId);
                                if (completableFuture != null)
                                    completableFuture.SetResult(true);
                            }
                        }
                        catch (Exception exception)
                        {
                            Log.Warning($"Bad start notification: {exception}");
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
                                    instanceInfo.instanceId,
                                    instanceInfo.playerId,
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
                            if (instances.JavaRemove(instanceId) != null)
                                Log.Information($"Buildplate instance {instanceId} has stopped");
                        }
                    }
                    break;
            }
        }

        private record StartRequest(
            string playerId,
            string buildplateId,
            bool survival,
            bool night
        )
        {
        }

        private record StartNotification(
            string instanceId,
            string playerId,
            string buildplateId,
            string address,
            int port
        )
        {
        }

        public record InstanceInfo(
            string instanceId,

            string playerId,
            string buildplateId,

            string address,
            int port,

            bool ready
        )
        {
        }
    }
}
