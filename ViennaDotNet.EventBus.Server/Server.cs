using Serilog;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using ViennaDotNet.Common.Utils;

namespace ViennaDotNet.EventBus.Server;

public partial class Server
{
    private readonly ReaderWriterLockSlim subscribersLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    private readonly Dictionary<string, HashSet<Subscriber>> subscribers = [];

    private readonly ReaderWriterLockSlim requestHandlersLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    private readonly Dictionary<string, HashSet<RequestHandler>> requestHandlers = [];

    public Subscriber? addSubscriber(string queueName, Action<Subscriber.Message> consumer)
    {
        if (!validateQueueName(queueName))
            return null;

        Log.Debug($"Adding subscriber for {queueName}");

        subscribersLock.EnterWriteLock();

        Subscriber subscriber = new Subscriber(this, queueName, consumer);
        subscribers.ComputeIfAbsent(queueName, name => [])!.Add(subscriber);

        subscribersLock.ExitWriteLock();

        return subscriber;
    }

    public sealed class Subscriber
    {
        private readonly Server server;

        private readonly string queueName;
        private readonly Action<Message> consumer;
        private bool ended = false;

        internal Subscriber(Server server, string queueName, Action<Message> consumer)
        {
            this.server = server;
            this.queueName = queueName;
            this.consumer = consumer;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void remove()
        {
            ended = true;

            new Thread(() =>
            {
                Log.Debug("Removing subscriber");
                server.subscribersLock.EnterWriteLock();
                HashSet<Subscriber>? subscribers = server.subscribers.GetOrDefault(queueName, null);
                subscribers?.Remove(this);

                server.subscribersLock.ExitWriteLock();
            }).Start();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void push(EntryMessage entryMessage)
        {
            if (!ended)
                consumer.Invoke(entryMessage);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void error()
        {
            if (!ended)
            {
                consumer.Invoke(new ErrorMessage());
                ended = true;
            }
        }

        public abstract class Message
        {
            protected Message()
            {
                // empty
            }
        }

        public sealed class EntryMessage : Message
        {
            public readonly long timestamp;
            public readonly string type;
            public readonly string data;

            internal EntryMessage(long timestamp, string type, string data)
            {
                this.timestamp = timestamp;
                this.type = type;
                this.data = data;
            }
        }

        public class ErrorMessage : Message
        {
            internal ErrorMessage()
            {
                // empty
            }
        }
    }

    private HashSet<Subscriber> getSubscribers(string queueName)
    {
        HashSet<Subscriber>? subscribers = this.subscribers.GetOrDefault(queueName, null);
        return subscribers is not null
            ? subscribers
            : [];
    }

    public Publisher addPublisher()
    {
        Log.Debug("Adding publisher");
        return new Publisher(this);
    }

    public sealed class Publisher
    {
        private readonly Server server;
        private bool closed = false;

        public Publisher(Server server)
        {
            this.server = server;
        }

        public void remove()
        {
            Log.Debug("Removing publisher");
            closed = true;
        }

        public bool publish(string queueName, long timestamp, string type, string data)
        {
            if (closed)
                throw new Exception();

            if (!validateQueueName(queueName))
                return false;

            if (!validateType(type))
                return false;

            if (!validateData(data))
                return false;

            server.subscribersLock.EnterReadLock();

            Subscriber.EntryMessage message = new Subscriber.EntryMessage(timestamp, type, data);
            foreach (var subscriber in server.getSubscribers(queueName))
            {
                subscriber.push(message);
            }

            server.subscribersLock.ExitReadLock();

            return true;
        }
    }

    public Server.RequestHandler? addRequestHandler(string queueName, Func<RequestHandler.Request, TaskCompletionSource<string?>> requestHandler, Action<RequestHandler.ErrorMessage> errorConsumer)
    {
        if (!validateQueueName(queueName))
            return null;

        Log.Debug($"Adding request handler for {queueName}");

        requestHandlersLock.EnterWriteLock();

        RequestHandler handler = new RequestHandler(this, queueName, requestHandler, errorConsumer);
        requestHandlers.ComputeIfAbsent(queueName, name => [])!.Add(handler);

        requestHandlersLock.ExitWriteLock();

        return handler;
    }

    public sealed class RequestHandler
    {
        private readonly Server server;

        private readonly string queueName;
        private readonly Func<Request, TaskCompletionSource<string?>> requestHandler;
        private readonly Action<ErrorMessage> errorConsumer;
        private bool ended = false;

        internal RequestHandler(Server server, string queueName, Func<Request, TaskCompletionSource<string?>> requestHandler, Action<ErrorMessage> errorConsumer)
        {
            this.server = server;
            this.queueName = queueName;
            this.requestHandler = requestHandler;
            this.errorConsumer = errorConsumer;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void remove()
        {
            ended = true;

            new Thread(() =>
            {
                Log.Debug("Removing handler");
                server.requestHandlersLock.EnterWriteLock();
                HashSet<RequestHandler>? requestHandlers = server.requestHandlers.GetOrDefault(queueName, null);
                requestHandlers?.Remove(this);

                server.requestHandlersLock.ExitWriteLock();
            }).Start();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal TaskCompletionSource<string?> request(Request request)
        {
            if (!ended)
                return requestHandler.Invoke(request);
            else
            {
                var source = new TaskCompletionSource<string?>();
                source.SetResult(null);
                return source;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void error()
        {
            if (!ended)
            {
                errorConsumer.Invoke(new ErrorMessage());
                ended = true;
            }
        }

        public sealed class Request
        {
            public readonly long timestamp;
            public readonly string type;
            public readonly string data;

            internal Request(long timestamp, string type, string data)
            {
                this.timestamp = timestamp;
                this.type = type;
                this.data = data;
            }
        }

        public sealed class ErrorMessage
        {
            internal ErrorMessage()
            {
                // empty
            }
        }
    }

    private HashSet<RequestHandler> getHandlers(string queueName)
    {
        HashSet<RequestHandler>? requestHandlers = this.requestHandlers.GetOrDefault(queueName, null);
        return requestHandlers is not null
            ? requestHandlers
            : [];
    }

    public RequestSender addRequestSender()
    {
        Log.Debug("Adding request sender");
        return new RequestSender(this);
    }

    public sealed class RequestSender
    {
        private readonly Server server;

        private bool closed = false;

        internal RequestSender(Server server)
        {
            this.server = server;
        }

        public void remove()
        {
            Log.Debug("Removing request sender");
            closed = true;
        }

        public TaskCompletionSource<string?>? request(string queueName, long timestamp, string type, string data)
        {
            if (closed)
                throw new InvalidOperationException();

            if (!validateQueueName(queueName))
                return null;

            if (!validateType(type))
                return null;

            if (!validateData(data))
                return null;

            server.requestHandlersLock.EnterReadLock();
            LinkedList<RequestHandler> requestHandlers = server.getHandlers(queueName).Collect(() => new LinkedList<RequestHandler>(), (list, item) => list.AddLast(item), (l1, l2) => l1.AddRange(l2));
            server.requestHandlersLock.ExitReadLock();

            RequestHandler.Request request = new RequestHandler.Request(timestamp, type, data);
            TaskCompletionSource<string?> responseCompletableFuture = new();

            new Thread(() =>
            {
                foreach (RequestHandler requestHandler in requestHandlers)
                {
                    TaskCompletionSource<string?> completableFuture = requestHandler.request(request);
                    string? response = completableFuture.Task.Result;
                    if (response is not null)
                    {
                        responseCompletableFuture.SetResult(response);
                        break;
                    }
                }

                responseCompletableFuture./*SetResult*/TrySetResult(null);
            }).Start();

            return responseCompletableFuture;
        }
    }

    private static bool validateQueueName(string queueName)
        => !string.IsNullOrWhiteSpace(queueName) && queueName.Length != 0 && !GetValitationRegex1().IsMatch(queueName) && !GetValitationRegex2().IsMatch(queueName);

    private static bool validateType(string type)
        => !string.IsNullOrWhiteSpace(type) && type.Length != 0 && !GetValitationRegex1().IsMatch(type) && !GetValitationRegex2().IsMatch(type);

    private static bool validateData(string str)
    {
        for (int i = 0; i < str.Length; i++)
            if (str[i] < 32 || str[i] >= 127)
                return false;

        return true;
    }

    [GeneratedRegex("[^A-Za-z0-9_\\-]")]
    private static partial Regex GetValitationRegex1();

    [GeneratedRegex("^[^A-Za-z0-9]")]
    private static partial Regex GetValitationRegex2();
}
