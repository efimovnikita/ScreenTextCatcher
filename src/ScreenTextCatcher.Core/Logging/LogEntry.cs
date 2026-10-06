namespace ScreenTextCatcher.Core.Logging;

public record LogEntry(
    DateTime Timestamp,
    string Level,
    string Message,
    string? Exception = null
);
