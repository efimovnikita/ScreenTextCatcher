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

        GenerateIcons();

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

    private void GenerateIcons()
    {
        _idleIcon = CreateSolidColorIcon(Color.FromArgb(0, 150, 255), "T");
        _busyIcon = CreateSolidColorIcon(Color.FromArgb(255, 180, 0), "⏳");
        _errorIcon = CreateSolidColorIcon(Color.FromArgb(240, 50, 50), "!");
    }

    private static Icon CreateSolidColorIcon(Color bg, string label)
    {
        using var bmp = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(bg);
            g.FillEllipse(brush, 2, 2, 28, 28);

            using var font = new Font("Arial", 14, FontStyle.Bold);
            using var textBrush = new SolidBrush(Color.White);
            using var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            g.DrawString(label, font, textBrush, new RectangleF(0, 0, 32, 32), sf);
        }

        var hIcon = bmp.GetHicon();
        return Icon.FromHandle(hIcon);
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
        _busyIcon?.Dispose();
        _errorIcon?.Dispose();
    }
}
