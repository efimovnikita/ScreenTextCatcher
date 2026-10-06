using System.IO;
using FluentAssertions;
using ScreenTextCatcher.Core.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;

namespace ScreenTextCatcher.Tests;

public class LoggingTests : IDisposable
{
    private readonly string _tempLogDir;

    public LoggingTests()
    {
        _tempLogDir = Path.Combine(Path.GetTempPath(), "ScreenTextCatcher_Logs_" + Guid.NewGuid().ToString("N"));
    }

    public void Dispose()
    {
        AppLogger.CloseAndFlush();
        if (Directory.Exists(_tempLogDir))
        {
            try
            {
                Directory.Delete(_tempLogDir, true);
            }
            catch
            {
                // Ignore cleanup lock in temp dir
            }
        }
    }

    [Fact]
    public void InMemoryLogSink_CapturesEntriesAndLimitsCapacity()
    {
        var sink = new InMemoryLogSink(maxEntries: 3);
        LogEntry? lastReceived = null;
        sink.LogEmitted += entry => lastReceived = entry;

        var parser = new MessageTemplateParser();
        for (int i = 1; i <= 5; i++)
        {
            var template = parser.Parse($"Message {i}");
            var logEvent = new LogEvent(
                DateTimeOffset.Now,
                LogEventLevel.Information,
                null,
                template,
                Enumerable.Empty<LogEventProperty>());
            sink.Emit(logEvent);
        }

        var entries = sink.GetEntries();
        entries.Should().HaveCount(3);
        entries.Last().Message.Should().Be("Message 5");
        lastReceived.Should().NotBeNull();
        lastReceived!.Message.Should().Be("Message 5");
    }

    [Fact]
    public void AppLogger_Initialize_CreatesLogDirectory()
    {
        AppLogger.Initialize(_tempLogDir);

        Directory.Exists(_tempLogDir).Should().BeTrue();
        AppLogger.LogDirectory.Should().Be(_tempLogDir);

        Log.Information("Test initialization log");
        AppLogger.CloseAndFlush();

        var files = Directory.GetFiles(_tempLogDir, "app-*.log");
        files.Should().NotBeEmpty();
    }
}
