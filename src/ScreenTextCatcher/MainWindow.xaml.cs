using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using ScreenTextCatcher.Core.Logging;
using ScreenTextCatcher.Core.Models;
using ScreenTextCatcher.Core.Services;
using ScreenTextCatcher.Hotkeys;
using ScreenTextCatcher.Tray;
using ScreenTextCatcher.Views;
using Serilog;

namespace ScreenTextCatcher;

public partial class MainWindow : Window
{
    private readonly IConfigurationService _configService;
    private readonly IHistoryService _historyService;
    private readonly ScreenCaptureService _captureService;
    private readonly IAutoStartService _autoStartService;
    private readonly IScreenshotService _screenshotService;

    private TrayIconManager? _trayManager;
    private HotkeyManager? _hotkeyManager;
    private bool _isExiting = false;
    private int _sampleCount = 0;

    public MainWindow()
    {
        AppLogger.Initialize();
        Log.Information("Запуск ScreenTextCatcher");

        _configService = new ConfigurationService();
        _historyService = new HistoryService();
        _captureService = new ScreenCaptureService();
        _autoStartService = new AutoStartService();
        _screenshotService = new ScreenshotService();

        // Synchronize autostart shortcut if enabled in settings
        if (_configService.CurrentSettings.AutoStart && !_autoStartService.IsAutoStartEnabled())
        {
            Log.Information("Автозапуск включен в настройках, но ярлык отсутствует или не актуален. Обновление ярлыка...");
            _autoStartService.EnableAutoStart();
        }

        LocalizationManager.SetLanguage(_configService.CurrentSettings.Language);
        LocalizationManager.LanguageChanged += UpdateUiState;

        InitializeComponent();

        LoadSettingsToInputs();
        UpdateUiState();

        // Subscribe to real-time logs
        AppLogger.InMemorySink.LogEmitted += OnLogReceived;

        // Initialize Tray Manager
        _trayManager = new TrayIconManager(_historyService, _configService, _autoStartService);
        _trayManager.CaptureRequested += () => Dispatcher.Invoke(StartOcrCapture);
        _trayManager.SettingsRequested += () => Dispatcher.Invoke(OpenSettingsDialog);
        _trayManager.HistoryRequested += () => Dispatcher.Invoke(OpenHistoryDialog);
        _trayManager.LogsRequested += () => Dispatcher.Invoke(OpenLogsDialog);
        _trayManager.DashboardRequested += () => Dispatcher.Invoke(ShowDashboard);
        _trayManager.ExitRequested += () => Dispatcher.Invoke(ExitApplication);
        _trayManager.ModeChanged += mode => Dispatcher.Invoke(() =>
        {
            SyncModeRadioButtons(mode);
            UpdateUiState();
        });

        Closing += OnMainWindowClosing;

        // Force HWND creation in background without showing the window
        new WindowInteropHelper(this).EnsureHandle();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        if (_hotkeyManager != null) return;

        var handle = new WindowInteropHelper(this).Handle;
        _hotkeyManager = new HotkeyManager();
        _hotkeyManager.Initialize(handle);

        var s = _configService.CurrentSettings;
        _hotkeyManager.Register(s.Hotkey, () => Dispatcher.Invoke(StartOcrCapture));
    }

    private void OnMainWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (!_isExiting)
        {
            e.Cancel = true;
            Hide();
            ShowInTaskbar = false;
            Log.Information("Окно тихо свернуто в системный трей (без уведомлений).");
        }
    }

    public void ShowDashboard()
    {
        ShowInTaskbar = true;
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    public void ExitApplication()
    {
        _isExiting = true;
        _hotkeyManager?.Dispose();
        _trayManager?.Dispose();
        AppLogger.CloseAndFlush();
        System.Windows.Application.Current.Shutdown();
    }

    private void LoadSettingsToInputs()
    {
        var s = _configService.CurrentSettings;
        SyncModeRadioButtons(s.Mode);
        TxtApiKey.Text = s.MistralApiKey;
        ChkProxyEnabled.IsChecked = s.Proxy.Enabled;
        CmbProxyType.SelectedIndex = (int)s.Proxy.Type;
        TxtProxyHost.Text = s.Proxy.Host;
        TxtProxyPort.Text = s.Proxy.Port.ToString();
        TxtProxyUser.Text = s.Proxy.Username;
        TxtProxyPassword.Password = s.Proxy.Password;
    }

    private void SyncModeRadioButtons(AppMode mode)
    {
        if (mode == AppMode.Screenshot)
        {
            RbDashboardModeScreenshot.IsChecked = true;
        }
        else
        {
            RbDashboardModeOcr.IsChecked = true;
        }
    }

    private void OnDashboardModeChanged(object sender, RoutedEventArgs e)
    {
        if (_trayManager == null || _configService == null) return;
        var newMode = RbDashboardModeScreenshot.IsChecked == true ? AppMode.Screenshot : AppMode.Ocr;
        _trayManager.SetActiveMode(newMode);
        UpdateUiState();
    }

    private void OnOpenScreenshotsFolderClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var folder = _configService.CurrentSettings.GetEffectiveScreenshotFolder();
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            Process.Start(new ProcessStartInfo
            {
                FileName = folder,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(ex.Message, LocalizationManager.GetString("Loc_NotificationScreenshotErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UpdateUiState()
    {
        var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ScreenTextCatcher");
        TxtHistoryPath.Text = Path.Combine(appData, "history.db");
        var format = LocalizationManager.GetString("Loc_HistoryCountFormat");
        TxtHistoryCount.Text = string.Format(format, _historyService.Count());

        var isScreenshot = _configService.CurrentSettings.Mode == AppMode.Screenshot;
        if (BtnCapture != null)
        {
            BtnCapture.Content = LocalizationManager.GetString(isScreenshot ? "Loc_TrayCaptureScreenshot" : "Loc_BtnCapture");
        }
    }

    private void OnSaveConfigClick(object sender, RoutedEventArgs e)
    {
        var s = _configService.CurrentSettings;
        s.MistralApiKey = TxtApiKey.Text.Trim();
        s.Proxy.Enabled = ChkProxyEnabled.IsChecked == true;
        s.Proxy.Type = CmbProxyType.SelectedIndex >= 0 ? (ProxyType)CmbProxyType.SelectedIndex : ProxyType.Http;
        s.Proxy.Host = TxtProxyHost.Text.Trim();

        if (int.TryParse(TxtProxyPort.Text.Trim(), out var port))
        {
            s.Proxy.Port = port;
        }

        s.Proxy.Username = TxtProxyUser.Text.Trim();
        s.Proxy.Password = TxtProxyPassword.Password;

        _configService.Save(s);
        Log.Information("Настройки сохранены! Прокси: {ProxyState}, API-ключ: {KeyStatus}",
            s.Proxy.Enabled ? $"{s.Proxy.Type}://{s.Proxy.Host}:{s.Proxy.Port}" : "Отключен",
            string.IsNullOrWhiteSpace(s.MistralApiKey) ? "Не задан" : "Задан");

        // Re-register hotkey if needed
        _hotkeyManager?.Register(s.Hotkey, () => Dispatcher.Invoke(StartOcrCapture));

        UpdateUiState();
    }

    public async void StartOcrCapture()
    {
        Log.Information("Активация режима захвата экрана...");
        _trayManager?.SetStatus(TrayStatus.Processing, LocalizationManager.GetString("Loc_TraySelectArea"));

        bool wasVisible = IsVisible;
        if (wasVisible)
        {
            Hide();
            await Task.Delay(200);
        }

        var isScreenshot = _configService.CurrentSettings.Mode == AppMode.Screenshot;
        var overlay = new Views.OverlayWindow(isScreenshot);

        overlay.Cancelled += () =>
        {
            Dispatcher.Invoke(() =>
            {
                _trayManager?.SetStatus(TrayStatus.Idle);
                if (wasVisible)
                {
                    ShowDashboard();
                }
                Log.Information("Выбор фрагмента экрана отменен (Esc / ПКМ).");
            });
        };

        if (isScreenshot)
        {
            overlay.ScreenshotReady += pngBytes =>
            {
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        var s = _configService.CurrentSettings;
                        Log.Information("Режим скриншотов: сохранение области с аннотациями на диск и аккумуляция в буфере обмена...");
                        var folder = s.GetEffectiveScreenshotFolder();
                        var savedFilePath = _screenshotService.SaveAndAccumulateClipboard(pngBytes, folder, s.ClipboardDelimiter);
                        if (s.SoundFeedback)
                        {
                            System.Media.SystemSounds.Asterisk.Play();
                        }
                        _trayManager?.SetStatus(TrayStatus.Idle);
                        Log.Information(string.Format(LocalizationManager.GetString("Loc_LogScreenshotSaved"), savedFilePath));
                    }
                    catch (Exception ex)
                    {
                        _trayManager?.SetStatus(TrayStatus.Error, LocalizationManager.GetString("Loc_NotificationScreenshotErrorTitle"));
                        Log.Error(ex, "Ошибка сохранения скриншота: {Message}", ex.Message);
                        _trayManager?.ShowNotification(
                            LocalizationManager.GetString("Loc_NotificationScreenshotErrorTitle"),
                            string.Format(LocalizationManager.GetString("Loc_NotificationScreenshotErrorMsg"), ex.Message),
                            ToolTipIcon.Error);
                    }
                    finally
                    {
                        if (wasVisible)
                        {
                            ShowDashboard();
                        }
                        UpdateUiState();
                    }
                });
            };
        }
        else
        {
            overlay.AreaSelected += async rect =>
            {
                try
                {
                    _trayManager?.SetStatus(TrayStatus.Processing, LocalizationManager.GetString("Loc_TrayRecognizing"));
                    Log.Information("Захват области экрана в RAM: X={X}, Y={Y}, W={W}, H={H}", rect.X, rect.Y, rect.Width, rect.Height);

                    var pngBytes = _captureService.CaptureRegionToPngBytes(rect);
                    var s = _configService.CurrentSettings;

                Log.Information("Снимок вырезан в RAM ({Bytes} байт). Запись на диск отсутствует.", pngBytes.Length);

                if (string.IsNullOrWhiteSpace(s.MistralApiKey))
                {
                    _trayManager?.SetStatus(TrayStatus.Error, LocalizationManager.GetString("Loc_TrayNoApiKey"));
                    Log.Warning("Ошибка: Mistral API-ключ не указан в настройках.");
                    _trayManager?.ShowNotification(
                        LocalizationManager.GetString("Loc_NotificationErrorTitle"),
                        LocalizationManager.GetString("Loc_NotificationErrorKey"),
                        ToolTipIcon.Warning);
                    return;
                }

                Log.Information("Отправка запроса в Mistral OCR API...");
                var client = new MistralOcrClient(s);
                var ocrRes = await client.RecognizeTextAsync(pngBytes);

                if (ocrRes.Success)
                {
                    Log.Information("OCR успешно завершен ({Ms} мс): \"{Text}\"", ocrRes.ElapsedMilliseconds, ocrRes.Text);

                    Dispatcher.Invoke(() =>
                    {
                        System.Windows.Clipboard.SetText(ocrRes.Text);
                        if (s.SoundFeedback)
                        {
                            System.Media.SystemSounds.Asterisk.Play();
                        }
                    });

                    _historyService.Add(ocrRes.Text);
                    _trayManager?.SetStatus(TrayStatus.Idle);
                    Log.Information("Распознанный текст успешно помещен в буфер обмена Windows (Ctrl+V)!");
                }
                else
                {
                    _trayManager?.SetStatus(TrayStatus.Error, LocalizationManager.GetString("Loc_TrayOcrError"));
                    Log.Error("Ошибка Mistral OCR: {Err}", ocrRes.ErrorMessage);
                    _trayManager?.ShowNotification(
                        LocalizationManager.GetString("Loc_NotificationOcrErrorTitle"),
                        ocrRes.ErrorMessage ?? "Error",
                        ToolTipIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _trayManager?.SetStatus(TrayStatus.Error, LocalizationManager.GetString("Loc_TrayCaptureFail"));
                Log.Error(ex, "Исключение при захвате и распознавании");
            }
            finally
            {
                Dispatcher.Invoke(() =>
                {
                    if (wasVisible)
                    {
                        ShowDashboard();
                    }
                    UpdateUiState();
                });
            }
        };
        }

        overlay.Show();
    }

    private void OnTestOverlayClick(object sender, RoutedEventArgs e)
    {
        StartOcrCapture();
    }

    private void OnLogReceived(LogEntry entry)
    {
        Dispatcher.Invoke(() =>
        {
            AppendLogLine($"[{entry.Timestamp:HH:mm:ss}] [{entry.Level}] {entry.Message}");
        });
    }

    private void AppendLogLine(string text)
    {
        TxtLogs.AppendText(text + Environment.NewLine);
        TxtLogs.ScrollToEnd();
    }

    private void OnAddHistoryClick(object sender, RoutedEventArgs e)
    {
        _sampleCount++;
        var sampleText = $"Распознанный фрагмент #{_sampleCount} ({DateTime.Now:HH:mm:ss}): Тестовый текст из Mistral OCR.";
        _historyService.Add(sampleText);
        Log.Information("Добавлена новая запись в историю SQLite: '{Text}'", sampleText);
        UpdateUiState();
    }

    private void OnAddLogClick(object sender, RoutedEventArgs e)
    {
        Log.Information("Пользователь нажал кнопку логирования. Время: {Time:HH:mm:ss}", DateTime.Now);
    }

    private void OnOpenFolderClick(object sender, RoutedEventArgs e)
    {
        var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ScreenTextCatcher");
        if (Directory.Exists(appData))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = appData,
                UseShellExecute = true
            });
        }
        else
        {
            System.Windows.MessageBox.Show("Папка данных еще не создана.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private async void OnTestPingClick(object sender, RoutedEventArgs e)
    {
        OnSaveConfigClick(sender, e);

        Log.Information("Запуск проверки подключения к сети / Mistral API...");
        var tester = new ProxyTester();
        var s = _configService.CurrentSettings;

        if (s.Proxy.Enabled)
        {
            Log.Information("Тестирование прокси-сервера {Type}://{Host}:{Port}...", s.Proxy.Type, s.Proxy.Host, s.Proxy.Port);
            var proxyRes = await tester.TestProxyConnectionAsync(s.Proxy);
            if (proxyRes.Success)
            {
                Log.Information("Прокси: {Msg}", proxyRes.Message);
            }
            else
            {
                Log.Error("Прокси: {Msg}", proxyRes.Message);
            }
        }

        var apiRes = await tester.TestMistralApiAsync(s.MistralApiKey, s.Proxy);
        if (apiRes.Success)
        {
            Log.Information("Mistral API: {Msg}", apiRes.Message);
        }
        else
        {
            Log.Warning("Mistral API: {Msg}", apiRes.Message);
        }
    }

    private async void OnTestOcrClick(object sender, RoutedEventArgs e)
    {
        OnSaveConfigClick(sender, e);

        var s = _configService.CurrentSettings;
        if (string.IsNullOrWhiteSpace(s.MistralApiKey))
        {
            Log.Warning("Для тестирования OCR требуется ввести Mistral API-ключ в поле выше и сохранить его.");
            return;
        }

        Log.Information("Генерация тестового изображения в RAM (300x80) и отправка в Mistral OCR...");
        try
        {
            using var bmp = new System.Drawing.Bitmap(300, 80);
            using (var g = System.Drawing.Graphics.FromImage(bmp))
            {
                g.Clear(System.Drawing.Color.White);
                using var font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold);
                using var brush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                g.DrawString("Mistral OCR Test!", font, brush, 10, 25);
            }

            using var ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            var bytes = ms.ToArray();

            var client = new MistralOcrClient(s);
            var result = await client.RecognizeTextAsync(bytes);

            if (result.Success)
            {
                Log.Information("Mistral OCR успешно вернул текст ({Elapsed} мс): \"{Text}\"", result.ElapsedMilliseconds, result.Text);
                _historyService.Add(result.Text);
                UpdateUiState();
            }
            else
            {
                Log.Error("Ошибка Mistral OCR: {Err}", result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка выполнения теста OCR");
        }
    }

    public void OpenSettingsDialog()
    {
        var settingsWin = new SettingsWindow(_configService, _autoStartService);
        if (IsVisible)
        {
            settingsWin.Owner = this;
        }

        settingsWin.SettingsSaved += () =>
        {
            var s = _configService.CurrentSettings;
            _hotkeyManager?.Register(s.Hotkey, () => Dispatcher.Invoke(StartOcrCapture));
            LoadSettingsToInputs();
            UpdateUiState();
            _trayManager?.UpdateModeMenuItems();
            Log.Information("Настройки обновлены через окно настроек.");
        };

        settingsWin.ShowDialog();
    }

    public void OpenHistoryDialog()
    {
        var historyWin = new HistoryWindow(_historyService);
        if (IsVisible)
        {
            historyWin.Owner = this;
        }

        historyWin.ShowDialog();
        UpdateUiState();
    }

    public void OpenLogsDialog()
    {
        var logWin = new LogViewerWindow();
        if (IsVisible)
        {
            logWin.Owner = this;
        }

        logWin.Show();
    }

    private void OnOpenSettingsClick(object sender, RoutedEventArgs e) => OpenSettingsDialog();
    private void OnOpenHistoryClick(object sender, RoutedEventArgs e) => OpenHistoryDialog();
    private void OnOpenLogsClick(object sender, RoutedEventArgs e) => OpenLogsDialog();
}