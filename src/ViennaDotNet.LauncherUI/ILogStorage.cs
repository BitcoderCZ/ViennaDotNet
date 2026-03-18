using System.Collections.Concurrent;

namespace ViennaDotNet.LauncherUI;

public interface ILogStorage
{
    void AddLogs(LogEvent[] logs);
    IEnumerable<LogEvent> GetLogs(DateTime? start, DateTime? end, string[] levels, string componentName);
}

public sealed class InMemoryLogStorage : ILogStorage
{
    // this could be more efficient
    private readonly ConcurrentDictionary<string, List<LogEvent>> _componentLogs = new();
    private readonly object _lock = new();
    private readonly int _maxCapacity;

    public event Action<IEnumerable<LogEvent>>? OnLogsAdded;

    public InMemoryLogStorage(int maxCapacity = 5000)
    {
        _maxCapacity = maxCapacity;
    }

    public void AddLogs(LogEvent[] logs)
    {
        if (logs.Length is 0)
        {
            return;
        }

        lock (_lock)
        {
            foreach (var log in logs)
            {
                var component = log.Properties?.ComponentName ?? "Unknown";

                if (!_componentLogs.TryGetValue(component, out var list))
                {
                    list = [];
                    _componentLogs[component] = list;
                }

                list.Add(log);

                // Housekeeping: Keep memory usage in check
                if (list.Count > _maxCapacity)
                {
                    list.RemoveRange(0, list.Count - _maxCapacity);
                }
            }
        }

        // Trigger the event outside the lock to avoid deadlocks in subscribers
        OnLogsAdded?.Invoke(logs);
    }

    public IEnumerable<LogEvent> GetLogs(DateTime? start, DateTime? end, string[] levels, string componentName)
    {
        if (string.IsNullOrEmpty(componentName) || !_componentLogs.TryGetValue(componentName, out var list))
        {
            return [];
        }

        List<LogEvent> snapshot;
        lock (_lock)
        {
            snapshot = [.. list];
        }

        var query = snapshot.AsEnumerable();

        if (start.HasValue)
        {
            query = query.Where(l => l.Timestamp >= start.Value);
        }

        if (end.HasValue)
            query = query.Where(l => l.Timestamp <= end.Value);

        if (levels != null && levels.Any())
            query = query.Where(l => levels.Contains(l.Level, StringComparer.OrdinalIgnoreCase));

        return query.OrderByDescending(l => l.Timestamp);
    }
}