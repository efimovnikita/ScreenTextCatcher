using ScreenTextCatcher.Core.Services;

namespace ScreenTextCatcher.Tests;

public class LocalizationManagerTests
{
    [Fact]
    public void SetLanguage_Ru_ReturnsRussianStrings()
    {
        LocalizationManager.SetLanguage("ru");

        Assert.Equal("ru", LocalizationManager.CurrentLanguage);
        Assert.Contains("Панель", LocalizationManager.GetString("Loc_MainWindowTitle"));
        Assert.Contains("Настройки", LocalizationManager.GetString("Loc_BtnSettings"));
        Assert.Contains("Захват", LocalizationManager.GetString("Loc_TrayCapture"));
    }

    [Fact]
    public void SetLanguage_En_ReturnsEnglishStrings()
    {
        LocalizationManager.SetLanguage("en");

        Assert.Equal("en", LocalizationManager.CurrentLanguage);
        Assert.Contains("Control Panel", LocalizationManager.GetString("Loc_MainWindowTitle"));
        Assert.Contains("Settings", LocalizationManager.GetString("Loc_BtnSettings"));
        Assert.Contains("Capture", LocalizationManager.GetString("Loc_TrayCapture"));
    }

    [Fact]
    public void SetLanguage_FiresLanguageChangedEvent()
    {
        bool eventFired = false;
        Action handler = () => eventFired = true;

        try
        {
            LocalizationManager.LanguageChanged += handler;
            LocalizationManager.SetLanguage("en");
            Assert.True(eventFired);
        }
        finally
        {
            LocalizationManager.LanguageChanged -= handler;
            LocalizationManager.SetLanguage("ru");
        }
    }

    [Fact]
    public void GetString_UnknownKey_ReturnsFallbackOrKey()
    {
        var res1 = LocalizationManager.GetString("Unknown_Key_123");
        Assert.Equal("Unknown_Key_123", res1);

        var res2 = LocalizationManager.GetString("Unknown_Key_456", "DefaultValue");
        Assert.Equal("DefaultValue", res2);
    }
}
