using FluentAssertions;
using ScreenTextCatcher.Core.Models;

namespace ScreenTextCatcher.Tests;

public class HotkeyTests
{
    [Fact]
    public void HotkeySettings_DefaultValues_AreWinShiftX()
    {
        var hotkey = new HotkeySettings();

        hotkey.Win.Should().BeTrue();
        hotkey.Shift.Should().BeTrue();
        hotkey.Ctrl.Should().BeFalse();
        hotkey.Alt.Should().BeFalse();
        hotkey.Key.Should().Be("X");
    }

    [Fact]
    public void HotkeySettings_CustomConfiguration_StoresAccurately()
    {
        var hotkey = new HotkeySettings
        {
            Win = false,
            Ctrl = true,
            Alt = true,
            Shift = false,
            Key = "C"
        };

        hotkey.Ctrl.Should().BeTrue();
        hotkey.Alt.Should().BeTrue();
        hotkey.Win.Should().BeFalse();
        hotkey.Shift.Should().BeFalse();
        hotkey.Key.Should().Be("C");
    }
}
