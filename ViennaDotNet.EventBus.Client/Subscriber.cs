namespace ViennaDotNet.EventBus.Client;

public sealed class Subscriber
{
    private EventBusClient client;
    int channelId;

    string queueName;

    private ISubscriberListener listener;

    internal Subscriber(EventBusClient client, int channelId, string queueName, ISubscriberListener listener)
    {
        this.client = client;
        this.channelId = channelId;
        this.queueName = queueName;
        this.listener = listener;
    }

    public void close()
    {
        client.removeSubscriber(channelId);
        client.sendMessage(channelId, "CLOSE");
    }

    internal async Task<bool> handleMessage(string message)
    {
        if (message == "ERR")
        {
            close();
            listener.Error();
            return true;
        }
        else
        {
            string[] fields = message.Split(':', 3);
            if (fields.Length != 3)
                return false;

            if (!long.TryParse(fields[0], out long timestamp) || timestamp < 0)
                return false;

            string type = fields[1];
            string data = fields[2];

            await listener.Event(new Event(timestamp, type, data));

            return true;
        }
    }

    internal void error()
    {
        listener.Error();
    }

    public interface ISubscriberListener
    {
        Task Event(Event _event);

        void Error();
    }

    public class SubscriberListener : ISubscriberListener
    {
        public Func<Event, Task>? OnEvent;
        public Action? OnError;

        public SubscriberListener()
        {
        }
        public SubscriberListener(Func<Event, Task>? _onEvent = null, Action? _onError = null)
        {
            OnEvent = _onEvent;
            OnError = _onError;
        }

        public void Error()
            => OnError?.Invoke();

        public Task Event(Event _event)
            => OnEvent?.Invoke(_event) ?? Task.CompletedTask;
    }

    public sealed class Event
    {
        public long timestamp;
        public string type;
        public string data;

        internal Event(long timestamp, string type, string data)
        {
            this.timestamp = timestamp;
            this.type = type;
            this.data = data;
        }
    }
}
