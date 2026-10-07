using System.IO;
using FluentAssertions;
using ScreenTextCatcher.Core.Services;

namespace ScreenTextCatcher.Tests;

public class AutoStartServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _fakeExePath;
    private readonly string _shortcutPath;

    public AutoStartServiceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "ScreenTextCatcher_AutoStartTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
        _fakeExePath = Path.Combine(_tempDirectory, "ScreenTextCatcher.exe");
        File.WriteAllText(_fakeExePath, "dummy exe content");
        _shortcutPath = Path.Combine(_tempDirectory, "ScreenTextCatcher.lnk");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            try
            {
                Directory.Delete(_tempDirectory, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Fact]
    public void IsAutoStartEnabled_WhenShortcutDoesNotExist_ReturnsFalse()
    {
        var service = new AutoStartService(_tempDirectory, _fakeExePath);
        service.IsAutoStartEnabled().Should().BeFalse();
    }

    [Fact]
    public void EnableAutoStart_CreatesShortcutFileAndReturnsTrue()
    {
        var service = new AutoStartService(_tempDirectory, _fakeExePath);

        bool result = service.EnableAutoStart();

        result.Should().BeTrue();
        File.Exists(_shortcutPath).Should().BeTrue();
        service.IsAutoStartEnabled().Should().BeTrue();
    }

    [Fact]
    public void DisableAutoStart_WhenShortcutExists_RemovesFileAndReturnsTrue()
    {
        var service = new AutoStartService(_tempDirectory, _fakeExePath);
        service.EnableAutoStart().Should().BeTrue();
        File.Exists(_shortcutPath).Should().BeTrue();

        bool result = service.DisableAutoStart();

        result.Should().BeTrue();
        File.Exists(_shortcutPath).Should().BeFalse();
        service.IsAutoStartEnabled().Should().BeFalse();
    }

    [Fact]
    public void DisableAutoStart_WhenShortcutDoesNotExist_ReturnsTrue()
    {
        var service = new AutoStartService(_tempDirectory, _fakeExePath);
        bool result = service.DisableAutoStart();
        result.Should().BeTrue();
    }

    [Fact]
    public void SetAutoStart_TogglesCorrectly()
    {
        var service = new AutoStartService(_tempDirectory, _fakeExePath);

        service.SetAutoStart(true).Should().BeTrue();
        service.IsAutoStartEnabled().Should().BeTrue();
        File.Exists(_shortcutPath).Should().BeTrue();

        service.SetAutoStart(false).Should().BeTrue();
        service.IsAutoStartEnabled().Should().BeFalse();
        File.Exists(_shortcutPath).Should().BeFalse();
    }

    [Fact]
    public void EnableAutoStart_WhenStartupDirectoryDoesNotExist_CreatesDirectoryAndShortcut()
    {
        var nonExistentDir = Path.Combine(_tempDirectory, "SubStartup");
        var service = new AutoStartService(nonExistentDir, _fakeExePath);

        bool result = service.EnableAutoStart();

        result.Should().BeTrue();
        Directory.Exists(nonExistentDir).Should().BeTrue();
        File.Exists(Path.Combine(nonExistentDir, "ScreenTextCatcher.lnk")).Should().BeTrue();
        service.IsAutoStartEnabled().Should().BeTrue();
    }
}
