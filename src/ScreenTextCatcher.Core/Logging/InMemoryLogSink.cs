using System.Collections.Concurrent;
using Serilog.Core;
using Serilog.Events;

namespace ScreenTextCatcher.Core.Logging;

public class InMemoryLogSink : ILogEventSink
{
    private readonly int _maxEntries;
    private readonly ConcurrentQueue<LogEntry> _entries = new();

    public event Action<LogEntry>? LogEmitted;

    public InMemoryLogSink(int maxEntries = 500)
    {
        _maxEntries = maxEntries;
    }

    public IReadOnlyList<LogEntry> GetEntries()
    {
        return _entries.ToArray();
    }

    public void Clear()
    {
        _entries.Clear();
    }

    public void Emit(LogEvent logEvent)
    {
        var entry = new LogEntry(
            logEvent.Timestamp.LocalDateTime,
            logEvent.Level.ToString(),
            logEvent.RenderMessage(),
            logEvent.Exception?.ToString()
        );

        _entries.Enqueue(entry);
        while (_entries.Count > _maxEntries && _entries.TryDequeue(out _))
        {
        }

        LogEmitted?.Invoke(entry);
    }
}
