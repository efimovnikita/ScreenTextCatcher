using System.Diagnostics;
using System.IO;
using System.Windows;
using ScreenTextCatcher.Core.Logging;
using ScreenTextCatcher.Core.Services;

namespace ScreenTextCatcher.Views;

public partial class LogViewerWindow : Window
{
    public LogViewerWindow()
    {
        InitializeComponent();

        TxtLogPath.Text = AppLogger.LogDirectory;

        // Load existing logs
        foreach (var entry in AppLogger.InMemorySink.GetEntries())
        {
            AppendEntry(entry);
        }

        AppLogger.InMemorySink.LogEmitted += OnLogEmitted;
        Closed += (s, e) => AppLogger.InMemorySink.LogEmitted -= OnLogEmitted;
    }

    private void OnLogEmitted(LogEntry entry)
    {
        Dispatcher.Invoke(() => AppendEntry(entry));
    }

    private void AppendEntry(LogEntry entry)
    {
        var line = $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{entry.Level}] {entry.Message}";
        if (!string.IsNullOrEmpty(entry.Exception))
        {
            line += Environment.NewLine + entry.Exception;
        }

        TxtLogBox.AppendText(line + Environment.NewLine);
        TxtLogBox.ScrollToEnd();
    }

    private void OnCopyLogsClick(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(TxtLogBox.Text))
        {
            System.Windows.Clipboard.SetText(TxtLogBox.Text);
            System.Windows.MessageBox.Show(
                LocalizationManager.GetString("Loc_LogsCopied"),
                LocalizationManager.GetString("Loc_LogTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }

    private void OnOpenLogFolderClick(object sender, RoutedEventArgs e)
    {
        if (Directory.Exists(AppLogger.LogDirectory))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = AppLogger.LogDirectory,
                UseShellExecute = true
            });
        }
    }

    private void OnClearLogsClick(object sender, RoutedEventArgs e)
    {
        TxtLogBox.Clear();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
