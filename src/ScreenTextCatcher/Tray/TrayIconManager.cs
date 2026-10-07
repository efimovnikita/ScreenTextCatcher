using System.Drawing;
using System.Windows.Forms;
using ScreenTextCatcher.Core.Logging;
using ScreenTextCatcher.Core.Services;
using Serilog;

namespace ScreenTextCatcher.Tray;

public enum TrayStatus
{
    Idle,
    Processing,
    Error
}

public class TrayIconManager : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _contextMenu;
    private readonly IHistoryService _historyService;
    private readonly IConfigurationService _configService;
    private readonly IAutoStartService _autoStartService;

    private Icon? _idleIcon;
    private Icon? _busyIcon;
    private Icon? _errorIcon;

    public event Action? CaptureRequested;
    public event Action? HistoryRequested;
    public event Action? SettingsRequested;
    public event Action? LogsRequested;
    public event Action? DashboardRequested;
    public event Action? ExitRequested;
    public event Action<ScreenTextCatcher.Core.Models.AppMode>? ModeChanged;

    public TrayIconManager(
        IHistoryService historyService,
        IConfigurationService? configService = null,
        IAutoStartService? autoStartService = null)
    {
        _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        _configService = configService ?? new ConfigurationService();
        _autoStartService = autoStartService ?? new AutoStartService();

        LoadIcons();

        _contextMenu = new ContextMenuStrip();
        BuildContextMenu();

        _notifyIcon = new NotifyIcon
        {
            Icon = _idleIcon,
            Text = "ScreenTextCatcher (Win+Shift+X)",
            Visible = true,
            ContextMenuStrip = _contextMenu
        };

        _notifyIcon.DoubleClick += (s, e) => CaptureRequested?.Invoke();
        _contextMenu.Opening += (s, e) =>
        {
            RebuildHistorySubmenu();
            UpdateAutoStartMenuItem();
            UpdateModeMenuItems();
        };

        LocalizationManager.LanguageChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged()
    {
        BuildContextMenu();
    }

    private void LoadIcons()
    {
        _idleIcon = LoadIconResource("tray_idle.ico") ?? SystemIcons.Application;
        _busyIcon = LoadIconResource("tray_busy.ico") ?? _idleIcon;
        _errorIcon = LoadIconResource("tray_error.ico") ?? SystemIcons.Error;
    }

    private static Icon? LoadIconResource(string name)
    {
        try
        {
            var uri = new Uri($"pack://application:,,,/ScreenTextCatcher;component/Assets/{name}", UriKind.Absolute);
            var streamInfo = System.Windows.Application.GetResourceStream(uri);
            if (streamInfo?.Stream != null)
            {
                using var stream = streamInfo.Stream;
                return new Icon(stream);
            }
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Не удалось загрузить иконку {Name} из ресурсов сборки", name);
        }

        try
        {
            var localPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", name);
            if (System.IO.File.Exists(localPath))
            {
                return new Icon(localPath);
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Не удалось загрузить иконку {Name} из локального каталога Assets", name);
        }

        return null;
    }

    private void BuildContextMenu()
    {
        _contextMenu.Items.Clear();

        var isScreenshot = _configService.CurrentSettings.Mode == ScreenTextCatcher.Core.Models.AppMode.Screenshot;
        var captureItem = new ToolStripMenuItem(
            LocalizationManager.GetString(isScreenshot ? "Loc_TrayCaptureScreenshot" : "Loc_TrayCapture"),
            null,
            (s, e) => CaptureRequested?.Invoke())
        {
            Name = "CaptureItem",
            Font = new Font(_contextMenu.Font, FontStyle.Bold)
        };
        _contextMenu.Items.Add(captureItem);
        _contextMenu.Items.Add(new ToolStripSeparator());

        var ocrModeItem = new ToolStripMenuItem(LocalizationManager.GetString("Loc_TrayModeOcr"))
        {
            Name = "ModeOcrItem",
            Checked = !isScreenshot
        };
        ocrModeItem.Click += (s, e) => SwitchMode(ScreenTextCatcher.Core.Models.AppMode.Ocr);

        var screenshotModeItem = new ToolStripMenuItem(LocalizationManager.GetString("Loc_TrayModeScreenshot"))
        {
            Name = "ModeScreenshotItem",
            Checked = isScreenshot
        };
        screenshotModeItem.Click += (s, e) => SwitchMode(ScreenTextCatcher.Core.Models.AppMode.Screenshot);

        _contextMenu.Items.Add(ocrModeItem);
        _contextMenu.Items.Add(screenshotModeItem);
        _contextMenu.Items.Add(new ToolStripSeparator());

        var historyMenu = new ToolStripMenuItem(LocalizationManager.GetString("Loc_TrayRecent"));
        historyMenu.Name = "HistoryMenu";
        _contextMenu.Items.Add(historyMenu);
        _contextMenu.Items.Add(new ToolStripMenuItem(LocalizationManager.GetString("Loc_TrayAllHistory"), null, (s, e) => HistoryRequested?.Invoke()));

        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add(new ToolStripMenuItem(LocalizationManager.GetString("Loc_TraySettings"), null, (s, e) => SettingsRequested?.Invoke()));

        var autoStartItem = new ToolStripMenuItem(LocalizationManager.GetString("Loc_TrayAutoStart"))
        {
            Name = "AutoStartMenu",
            CheckOnClick = true,
            Checked = _autoStartService.IsAutoStartEnabled()
        };
        autoStartItem.Click += (s, e) =>
        {
            bool newStatus = autoStartItem.Checked;
            _autoStartService.SetAutoStart(newStatus);
            var settings = _configService.CurrentSettings;
            settings.AutoStart = newStatus;
            _configService.Save(settings);
            Log.Information("Автозапуск изменен через меню трея: {Status}", newStatus);
        };
        _contextMenu.Items.Add(autoStartItem);

        _contextMenu.Items.Add(new ToolStripMenuItem(LocalizationManager.GetString("Loc_TrayLogs"), null, (s, e) => LogsRequested?.Invoke()));
        _contextMenu.Items.Add(new ToolStripMenuItem(LocalizationManager.GetString("Loc_TrayShowDashboard"), null, (s, e) => DashboardRequested?.Invoke()));
        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add(new ToolStripMenuItem(LocalizationManager.GetString("Loc_TrayExit"), null, (s, e) => ExitRequested?.Invoke()));
    }

    public void SetActiveMode(ScreenTextCatcher.Core.Models.AppMode mode)
    {
        var settings = _configService.CurrentSettings;
        if (settings.Mode != mode)
        {
            settings.Mode = mode;
            _configService.Save(settings);
        }
        UpdateModeMenuItems();
        ModeChanged?.Invoke(mode);
    }

    private void SwitchMode(ScreenTextCatcher.Core.Models.AppMode mode)
    {
        SetActiveMode(mode);
        Log.Information("Режим работы переключен через меню трея: {Mode}", mode);
    }

    public void UpdateModeMenuItems()
    {
        var isScreenshot = _configService.CurrentSettings.Mode == ScreenTextCatcher.Core.Models.AppMode.Screenshot;
        if (_contextMenu.Items["ModeOcrItem"] is ToolStripMenuItem ocrItem)
        {
            ocrItem.Checked = !isScreenshot;
        }
        if (_contextMenu.Items["ModeScreenshotItem"] is ToolStripMenuItem scItem)
        {
            scItem.Checked = isScreenshot;
        }
        if (_contextMenu.Items["CaptureItem"] is ToolStripMenuItem captureItem)
        {
            captureItem.Text = LocalizationManager.GetString(isScreenshot ? "Loc_TrayCaptureScreenshot" : "Loc_TrayCapture");
        }
    }

    private void UpdateAutoStartMenuItem()
    {
        if (_contextMenu.Items["AutoStartMenu"] is ToolStripMenuItem item)
        {
            item.Checked = _autoStartService.IsAutoStartEnabled();
        }
    }

    private void RebuildHistorySubmenu()
    {
        if (_contextMenu.Items["HistoryMenu"] is not ToolStripMenuItem historyMenu)
        {
            return;
        }

        historyMenu.DropDownItems.Clear();
        var items = _historyService.GetRecent(10);

        if (items.Count == 0)
        {
            var emptyItem = new ToolStripMenuItem(LocalizationManager.GetString("Loc_TrayEmptyHistory")) { Enabled = false };
            historyMenu.DropDownItems.Add(emptyItem);
            return;
        }

        foreach (var item in items)
        {
            var menuItem = new ToolStripMenuItem(item.Preview, null, (s, e) =>
            {
                System.Windows.Clipboard.SetText(item.Text);
                Log.Information("Текст из истории скопирован в буфер обмена: \"{Preview}\"", item.Preview);
            })
            {
                ToolTipText = item.Text
            };
            historyMenu.DropDownItems.Add(menuItem);
        }
    }

    public void SetStatus(TrayStatus status, string? tooltipMessage = null)
    {
        _notifyIcon.Icon = status switch
        {
            TrayStatus.Processing => _busyIcon,
            TrayStatus.Error => _errorIcon,
            _ => _idleIcon
        };

        var text = string.IsNullOrWhiteSpace(tooltipMessage)
            ? "ScreenTextCatcher (Win+Shift+X)"
            : $"ScreenTextCatcher: {tooltipMessage}";

        if (text.Length > 63)
        {
            text = text.Substring(0, 60) + "...";
        }

        _notifyIcon.Text = text;
    }

    public void ShowNotification(string title, string text, ToolTipIcon icon = ToolTipIcon.Info)
    {
        _notifyIcon.ShowBalloonTip(3000, title, text, icon);
    }

    public void Dispose()
    {
        LocalizationManager.LanguageChanged -= OnLanguageChanged;

        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _contextMenu.Dispose();

        _idleIcon?.Dispose();
        if (!ReferenceEquals(_busyIcon, _idleIcon))
        {
            _busyIcon?.Dispose();
        }
        if (!ReferenceEquals(_errorIcon, _idleIcon))
        {
            _errorIcon?.Dispose();
        }
    }
}
