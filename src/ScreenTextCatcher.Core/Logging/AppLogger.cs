using System.IO;
using Serilog;

namespace ScreenTextCatcher.Core.Logging;

public static class AppLogger
{
    private static readonly InMemoryLogSink _inMemorySink = new();
    private static string _logDirectory = string.Empty;
    private static bool _initialized = false;

    public static InMemoryLogSink InMemorySink => _inMemorySink;
    public static string LogDirectory => _logDirectory;

    public static void Initialize(string? customLogDirectory = null)
    {
        if (_initialized)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(customLogDirectory))
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _logDirectory = Path.Combine(appData, "ScreenTextCatcher", "logs");
        }
        else
        {
            _logDirectory = customLogDirectory;
        }

        Directory.CreateDirectory(_logDirectory);

        var logFilePath = Path.Combine(_logDirectory, "app-.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Sink(_inMemorySink)
            .WriteTo.File(
                logFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        _initialized = true;
    }

    public static void CloseAndFlush()
    {
        Log.CloseAndFlush();
        _initialized = false;
    }
}
