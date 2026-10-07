using System.IO;
using FluentAssertions;
using ScreenTextCatcher.Core.Models;
using ScreenTextCatcher.Core.Services;

namespace ScreenTextCatcher.Tests;

public class ConfigurationServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _settingsFilePath;

    public ConfigurationServiceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "ScreenTextCatcher_Tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
        _settingsFilePath = Path.Combine(_tempDirectory, "settings.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Fact]
    public void Load_WhenFileDoesNotExist_ReturnsDefaultSettings()
    {
        var service = new ConfigurationService(_settingsFilePath);
        var settings = service.Load();

        settings.Should().NotBeNull();
        settings.MistralApiKey.Should().BeEmpty();
        settings.Proxy.Enabled.Should().BeFalse();
        settings.Proxy.Host.Should().Be("127.0.0.1");
        settings.Hotkey.Win.Should().BeTrue();
        settings.Hotkey.Shift.Should().BeTrue();
        settings.Hotkey.Key.Should().Be("X");
        settings.SoundFeedback.Should().BeTrue();
        settings.Language.Should().Be("ru");
        settings.AutoStart.Should().BeFalse();
        settings.Mode.Should().Be(AppMode.Ocr);
        settings.ScreenshotFolder.Should().BeEmpty();
        settings.ClipboardDelimiter.Should().Be(ClipboardDelimiter.NewLine);
        settings.GetEffectiveScreenshotFolder().Should().Contain("ScreenTextCatcher");
    }

    [Fact]
    public void Save_ThenLoad_PersistsAllSettingsCorrectly()
    {
        var service = new ConfigurationService(_settingsFilePath);
        var custom = new AppSettings
        {
            MistralApiKey = "test_api_key_12345",
            Mode = AppMode.Screenshot,
            ScreenshotFolder = @"C:\CustomScreenshots",
            ClipboardDelimiter = ClipboardDelimiter.Space,
            Proxy = new ProxySettings
            {
                Enabled = true,
                Type = ProxyType.Socks5,
                Host = "10.0.0.1",
                Port = 1080,
                Username = "user",
                Password = "pwd"
            },
            Hotkey = new HotkeySettings
            {
                Win = false,
                Alt = true,
                Shift = true,
                Key = "S"
            },
            SoundFeedback = false,
            Language = "en",
            AutoStart = true
        };

        service.Save(custom);

        File.Exists(_settingsFilePath).Should().BeTrue();

        var loaded = service.Load();
        loaded.MistralApiKey.Should().Be("test_api_key_12345");
        loaded.Mode.Should().Be(AppMode.Screenshot);
        loaded.ScreenshotFolder.Should().Be(@"C:\CustomScreenshots");
        loaded.ClipboardDelimiter.Should().Be(ClipboardDelimiter.Space);
        loaded.GetEffectiveScreenshotFolder().Should().Be(@"C:\CustomScreenshots");
        loaded.Proxy.Enabled.Should().BeTrue();
        loaded.Proxy.Type.Should().Be(ProxyType.Socks5);
        loaded.Proxy.Host.Should().Be("10.0.0.1");
        loaded.Proxy.Port.Should().Be(1080);
        loaded.Proxy.Username.Should().Be("user");
        loaded.Proxy.Password.Should().Be("pwd");
        loaded.Hotkey.Win.Should().BeFalse();
        loaded.Hotkey.Alt.Should().BeTrue();
        loaded.Hotkey.Shift.Should().BeTrue();
        loaded.Hotkey.Key.Should().Be("S");
        loaded.SoundFeedback.Should().BeFalse();
        loaded.Language.Should().Be("en");
        loaded.AutoStart.Should().BeTrue();
    }
}
