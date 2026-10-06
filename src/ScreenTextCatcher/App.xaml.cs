using System.IO;
using System.Windows;
using ScreenTextCatcher.Core.Logging;
using Serilog;

namespace ScreenTextCatcher;

public partial class App : System.Windows.Application
{
    public App()
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            var msg = e.ExceptionObject?.ToString() ?? "unknown";
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.txt"), msg);
            Log.Error("Unhandled exception: {Msg}", msg);
        };
        DispatcherUnhandledException += (s, e) =>
        {
            var msg = e.Exception?.ToString() ?? "unknown";
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.txt"), msg);
            Log.Error("Dispatcher unhandled exception: {Msg}", msg);
        };
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        AppLogger.Initialize();
        Log.Information("Запуск приложения ScreenTextCatcher в системном трее.");
        base.OnStartup(e);

        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("App OnExit triggered with code: {Code}", e.ApplicationExitCode);
        AppLogger.CloseAndFlush();
        base.OnExit(e);
    }
}
