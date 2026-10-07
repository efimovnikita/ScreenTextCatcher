using System.Windows;
using System.Windows.Media;
using ScreenTextCatcher.Core.Models;
using ScreenTextCatcher.Core.Services;
using Brushes = System.Windows.Media.Brushes;

namespace ScreenTextCatcher.Views;

public partial class SettingsWindow : Window
{
    private readonly IConfigurationService _configService;
    private readonly IAutoStartService _autoStartService;
    private readonly ProxyTester _proxyTester;

    public event Action? SettingsSaved;

    public SettingsWindow(IConfigurationService configService, IAutoStartService? autoStartService = null)
    {
        InitializeComponent();

        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _autoStartService = autoStartService ?? new AutoStartService();
        _proxyTester = new ProxyTester();

        LoadValues();
    }

    private void LoadValues()
    {
        var s = _configService.CurrentSettings;

        TxtApiKey.Text = s.MistralApiKey;
        ChkProxyEnabled.IsChecked = s.Proxy.Enabled;
        CmbProxyType.SelectedIndex = (int)s.Proxy.Type;
        TxtProxyHost.Text = s.Proxy.Host;
        TxtProxyPort.Text = s.Proxy.Port.ToString();
        TxtProxyUser.Text = s.Proxy.Username;
        TxtProxyPassword.Password = s.Proxy.Password;

        ChkWin.IsChecked = s.Hotkey.Win;
        ChkShift.IsChecked = s.Hotkey.Shift;
        ChkCtrl.IsChecked = s.Hotkey.Ctrl;
        ChkAlt.IsChecked = s.Hotkey.Alt;
        TxtKey.Text = string.IsNullOrWhiteSpace(s.Hotkey.Key) ? "X" : s.Hotkey.Key;

        ChkSound.IsChecked = s.SoundFeedback;
        ChkAutoStart.IsChecked = s.AutoStart;
        CmbLanguage.SelectedIndex = s.Language == "en" ? 1 : 0;
    }

    private async void OnTestPingClick(object sender, RoutedEventArgs e)
    {
        TxtTestStatus.Text = LocalizationManager.GetString("Loc_TestingConnection");
        TxtTestStatus.Foreground = Brushes.LightGray;

        var tempProxy = new ProxySettings
        {
            Enabled = ChkProxyEnabled.IsChecked == true,
            Type = CmbProxyType.SelectedIndex >= 0 ? (ProxyType)CmbProxyType.SelectedIndex : ProxyType.Http,
            Host = TxtProxyHost.Text.Trim(),
            Port = int.TryParse(TxtProxyPort.Text.Trim(), out var p) ? p : 8080,
            Username = TxtProxyUser.Text.Trim(),
            Password = TxtProxyPassword.Password
        };

        var key = TxtApiKey.Text.Trim();

        if (tempProxy.Enabled)
        {
            var proxyRes = await _proxyTester.TestProxyConnectionAsync(tempProxy);
            if (!proxyRes.Success)
            {
                TxtTestStatus.Text = string.Format(LocalizationManager.GetString("Loc_ProxyError"), proxyRes.Message);
                TxtTestStatus.Foreground = Brushes.Salmon;
                return;
            }
        }

        var apiRes = await _proxyTester.TestMistralApiAsync(key, tempProxy);
        if (apiRes.Success)
        {
            TxtTestStatus.Text = string.Format(LocalizationManager.GetString("Loc_TestSuccess"), apiRes.Message);
            TxtTestStatus.Foreground = Brushes.LightGreen;
        }
        else
        {
            TxtTestStatus.Text = string.Format(LocalizationManager.GetString("Loc_TestWarning"), apiRes.Message);
            TxtTestStatus.Foreground = Brushes.Khaki;
        }
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
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

        s.Hotkey.Win = ChkWin.IsChecked == true;
        s.Hotkey.Shift = ChkShift.IsChecked == true;
        s.Hotkey.Ctrl = ChkCtrl.IsChecked == true;
        s.Hotkey.Alt = ChkAlt.IsChecked == true;
        s.Hotkey.Key = string.IsNullOrWhiteSpace(TxtKey.Text) ? "X" : TxtKey.Text.Trim().ToUpperInvariant();

        s.SoundFeedback = ChkSound.IsChecked == true;
        bool autoStartEnabled = ChkAutoStart.IsChecked == true;
        s.AutoStart = autoStartEnabled;
        _autoStartService.SetAutoStart(autoStartEnabled);
        s.Language = CmbLanguage.SelectedIndex == 1 ? "en" : "ru";

        _configService.Save(s);
        LocalizationManager.SetLanguage(s.Language);
        SettingsSaved?.Invoke();

        Close();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
