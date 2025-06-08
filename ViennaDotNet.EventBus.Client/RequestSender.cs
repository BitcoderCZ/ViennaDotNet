using System.Text.RegularExpressions;
using ViennaDotNet.Common.Utils;

namespace ViennaDotNet.EventBus.Client;

public sealed class RequestSender
{
    private readonly EventBusClient client;
    private readonly int channelId;

    private readonly object _lock = new();

    private bool _closed = false;

    private readonly LinkedList<string> queuedRequests = new();
    private readonly LinkedList<TaskCompletionSource<string?>> queuedRequestResponses = new();
    private TaskCompletionSource<string?>? currentPendingResponse = null;

    internal RequestSender(EventBusClient client, int channelId)
    {
        this.client = client;
        this.channelId = channelId;
    }

    public void close()
    {
        client.removeRequestSender(channelId);
        client.sendMessage(channelId, "CLOSE");
        closed();
    }

    public TaskCompletionSource<string?> request(string queueName, string type, string data)
    {
        if (!validateQueueName(queueName))
            throw new ArgumentException("Queue name contains invalid characters");

        if (!validateType(type))
            throw new ArgumentException("Type contains invalid characters");

        if (!validateData(data))
            throw new ArgumentException("Data contains invalid characters");

        string requestMessage = "REQ " + queueName + ":" + type + ":" + data;

        TaskCompletionSource<string?> completableFuture = new();

        Monitor.Enter(_lock);
        if (_closed)
            completableFuture.SetResult(null);
        else
        {
            queuedRequests.AddLast(requestMessage);
            queuedRequestResponses.AddLast(completableFuture);
            if (currentPendingResponse == null)
                sendNextRequest();
        }

        Monitor.Exit(_lock);

        return completableFuture;
    }

    internal bool handleMessage(string message)
    {
        if (message == "ERR")
        {
            close();
            return true;
        }
        else if (message == "ACK")
            return true;
        else
        {
            string? response;

            string[] parts = message.Split(' ', 2);
            if (parts[0] == "NREP")
            {
                if (parts.Length != 1)
                    return false;

                response = null;
            }
            else if (parts[0] == "REP")
            {
                if (parts.Length != 2)
                    return false;

                response = parts[1];
            }
            else
                return false;

            try
            {
                Monitor.Enter(_lock);
                if (currentPendingResponse != null)
                {
                    currentPendingResponse.SetResult(response);
                    currentPendingResponse = null;
                    if (queuedRequests.Count != 0)
                        sendNextRequest();

                    return true;
                }
                else
                    return false;
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }
    }

    private void sendNextRequest()
    {
        string message = queuedRequests.First!.Value;
        queuedRequests.RemoveFirst();
        client.sendMessage(channelId, message);
        currentPendingResponse = queuedRequestResponses.First!.Value;
        queuedRequestResponses.RemoveFirst();
    }

    internal void closed()
    {
        Monitor.Enter(_lock);

        _closed = true;

        if (currentPendingResponse != null)
        {
            currentPendingResponse.TrySetResult(null);
            currentPendingResponse = null;
        }

        foreach (var completableFuture in queuedRequestResponses)
        {
            completableFuture.TrySetResult(null);
        }

        queuedRequestResponses.Clear();
        queuedRequests.Clear();
        Monitor.Exit(_lock);
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
