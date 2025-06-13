using System.Text.RegularExpressions;
using ViennaDotNet.Common.Utils;

namespace ViennaDotNet.EventBus.Client;

public sealed class Publisher
{
    private readonly EventBusClient client;
    private readonly int channelId;

    private readonly object _lock = new();

    private bool _closed = false;

    // TODO: probably should be a queue
    private readonly LinkedList<string> queuedEvents = new();
    private readonly LinkedList<TaskCompletionSource<bool>> queuedEventResults = new();
    private TaskCompletionSource<bool>? currentPendingEventResult = null;

    internal Publisher(EventBusClient client, int channelId)
    {
        this.client = client;
        this.channelId = channelId;
    }

    public void close()
    {
        client.removePublisher(channelId);
        client.sendMessage(channelId, "CLOSE");
        closed();
    }

    public Task<bool> publish(string queueName, string type, string data)
    {
        if (!validateQueueName(queueName))
            throw new ArgumentException("Queue name contains invalid characters", nameof(queueName));

        if (!validateType(type))
            throw new ArgumentException("Type contains invalid characters", nameof(type));

        if (!validateData(data))
            throw new ArgumentException("Data contains invalid characters", nameof(data));

        string eventMessage = "SEND " + queueName + ":" + type + ":" + data;

        TaskCompletionSource<bool> completableFuture = new TaskCompletionSource<bool>();

        lock (_lock)
        {
            if (_closed)
                completableFuture.SetResult(false);
            else
            {
                queuedEvents.AddLast(eventMessage);
                queuedEventResults.AddLast(completableFuture);
                if (currentPendingEventResult == null)
                    sendNextEvent();
            }
        }

        return completableFuture.Task;
    }

    public void flush()
    {
        Monitor.Enter(_lock);
        var task = queuedEventResults.Count == 0 ? currentPendingEventResult : queuedEventResults.Last!.Value;
        Monitor.Exit(_lock);

        if (task is not null)
        {
            task.Task.Wait();
        }
    }

    internal Task<bool> handleMessage(string message)
    {
        if (message == "ACK")
        {
            lock (_lock)
            {
                if (currentPendingEventResult != null)
                {
                    currentPendingEventResult.SetResult(true);
                    currentPendingEventResult = null;
                    if (!queuedEvents.IsEmpty())
                        sendNextEvent();

                    return Task.FromResult(true);
                }
                else
                    return Task.FromResult(false);
            }
        }
        else if (message == "ERR")
        {
            close();
            return Task.FromResult(true);
        }
        else
            return Task.FromResult(false);
    }

    private void sendNextEvent()
    {
        string message = queuedEvents.First!.Value;
        queuedEvents.RemoveFirst();
        client.sendMessage(channelId, message);
        currentPendingEventResult = queuedEventResults.First!.Value;
        queuedEventResults.RemoveFirst();
    }

    internal void closed()
    {
        lock (_lock)
        {
            _closed = true;

            if (currentPendingEventResult != null)
            {
                currentPendingEventResult.SetResult(false);
                currentPendingEventResult = null;
            }

            foreach (var task in queuedEventResults)
            {
                task.SetResult(false);
            }

            queuedEventResults.Clear();
            queuedEvents.Clear();
        }
    }

    private static bool validateQueueName(string queueName)
    {
        if (string.IsNullOrWhiteSpace(queueName) || queueName.Length == 0 || Regex.IsMatch(queueName, "[^A-Za-z0-9_\\-]") || Regex.IsMatch(queueName, "^[^A-Za-z0-9]"))
            return false;

        return true;
    }

    private static bool validateType(string type)
    {
        if (string.IsNullOrWhiteSpace(type) || type.Length == 0 || Regex.IsMatch(type, "[^A-Za-z0-9_\\-]") || Regex.IsMatch(type, "^[^A-Za-z0-9]"))
            return false;

        return true;
    }

    private static bool validateData(string str)
    {
        for (int i = 0; i < str.Length; i++)
            if (str[i] < 32 || str[i] >= 127)
                return false;

        return true;
    }
}
