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
        Assert.Equal("Режим работы приложения", LocalizationManager.GetString("Loc_ModeGroup"));
        Assert.Equal("Распознавание текста (Mistral OCR)", LocalizationManager.GetString("Loc_ModeOcr"));
        Assert.Equal("Сохранение скриншотов в папку", LocalizationManager.GetString("Loc_ModeScreenshot"));
        Assert.Equal("Папка для скриншотов:", LocalizationManager.GetString("Loc_ScreenshotFolderLabel"));
        Assert.Equal("Обзор...", LocalizationManager.GetString("Loc_BtnBrowse"));
        Assert.Equal("📂 Открыть папку скриншотов", LocalizationManager.GetString("Loc_BtnOpenScreenshotFolder"));
        Assert.Equal("✂ Сделать скриншот (Win+Shift+X)", LocalizationManager.GetString("Loc_TrayCaptureScreenshot"));
        Assert.Equal("Режим: Распознавание текста (OCR)", LocalizationManager.GetString("Loc_TrayModeOcr"));
        Assert.Equal("Режим: Сохранение скриншотов", LocalizationManager.GetString("Loc_TrayModeScreenshot"));
        Assert.Equal("Разделитель путей в буфере:", LocalizationManager.GetString("Loc_ClipboardDelimiterLabel"));
        Assert.Equal("Перенос строки (\\r\\n)", LocalizationManager.GetString("Loc_DelimiterNewLine"));
        Assert.Equal("Пробел (' ')", LocalizationManager.GetString("Loc_DelimiterSpace"));
    }

    [Fact]
    public void SetLanguage_En_ReturnsEnglishStrings()
    {
        LocalizationManager.SetLanguage("en");

        Assert.Equal("en", LocalizationManager.CurrentLanguage);
        Assert.Contains("Control Panel", LocalizationManager.GetString("Loc_MainWindowTitle"));
        Assert.Contains("Settings", LocalizationManager.GetString("Loc_BtnSettings"));
        Assert.Contains("Capture", LocalizationManager.GetString("Loc_TrayCapture"));
        Assert.Equal("Application Mode", LocalizationManager.GetString("Loc_ModeGroup"));
        Assert.Equal("Text Recognition (Mistral OCR)", LocalizationManager.GetString("Loc_ModeOcr"));
        Assert.Equal("Save Screenshots to Folder", LocalizationManager.GetString("Loc_ModeScreenshot"));
        Assert.Equal("Screenshots folder:", LocalizationManager.GetString("Loc_ScreenshotFolderLabel"));
        Assert.Equal("Browse...", LocalizationManager.GetString("Loc_BtnBrowse"));
        Assert.Equal("📂 Open Screenshots Folder", LocalizationManager.GetString("Loc_BtnOpenScreenshotFolder"));
        Assert.Equal("✂ Take Screenshot (Win+Shift+X)", LocalizationManager.GetString("Loc_TrayCaptureScreenshot"));
        Assert.Equal("Mode: Text Recognition (OCR)", LocalizationManager.GetString("Loc_TrayModeOcr"));
        Assert.Equal("Mode: Save Screenshots", LocalizationManager.GetString("Loc_TrayModeScreenshot"));
        Assert.Equal("Clipboard path delimiter:", LocalizationManager.GetString("Loc_ClipboardDelimiterLabel"));
        Assert.Equal("New line (\\r\\n)", LocalizationManager.GetString("Loc_DelimiterNewLine"));
        Assert.Equal("Space (' ')", LocalizationManager.GetString("Loc_DelimiterSpace"));
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

    [Fact]
    public void AnnotationToolbar_Keys_ExistInBothLanguages()
    {
        LocalizationManager.SetLanguage("ru");
        Assert.Equal("Рамка", LocalizationManager.GetString("Loc_AnnotateToolbarRect"));
        Assert.Equal("Стрелка", LocalizationManager.GetString("Loc_AnnotateToolbarArrow"));
        Assert.Equal("Отменить действие (Ctrl+Z)", LocalizationManager.GetString("Loc_AnnotateToolbarUndo"));
        Assert.Equal("Сохранить (Enter)", LocalizationManager.GetString("Loc_AnnotateToolbarDone"));
        Assert.Equal("Отмена (Esc)", LocalizationManager.GetString("Loc_AnnotateToolbarCancel"));

        LocalizationManager.SetLanguage("en");
        Assert.Equal("Rectangle", LocalizationManager.GetString("Loc_AnnotateToolbarRect"));
        Assert.Equal("Arrow", LocalizationManager.GetString("Loc_AnnotateToolbarArrow"));
        Assert.Equal("Undo (Ctrl+Z)", LocalizationManager.GetString("Loc_AnnotateToolbarUndo"));
        Assert.Equal("Save (Enter)", LocalizationManager.GetString("Loc_AnnotateToolbarDone"));
        Assert.Equal("Cancel (Esc)", LocalizationManager.GetString("Loc_AnnotateToolbarCancel"));
    }

    [Fact]
    public void AnnotationToolbarCopy_Keys_ExistInBothLanguages()
    {
        LocalizationManager.SetLanguage("ru");
        Assert.Equal("Копировать", LocalizationManager.GetString("Loc_AnnotateToolbarCopy"));
        Assert.Equal("Скопировать изображение в буфер обмена и сохранить на диск", LocalizationManager.GetString("Loc_AnnotateToolbarCopyTooltip"));
        Assert.Equal("Скриншот сохранён на диск и скопирован в буфер обмена как изображение: {0}", LocalizationManager.GetString("Loc_LogScreenshotImageCopied"));

        LocalizationManager.SetLanguage("en");
        Assert.Equal("Copy", LocalizationManager.GetString("Loc_AnnotateToolbarCopy"));
        Assert.Equal("Copy image to clipboard and save to disk", LocalizationManager.GetString("Loc_AnnotateToolbarCopyTooltip"));
        Assert.Equal("Screenshot saved to disk and copied to clipboard as image: {0}", LocalizationManager.GetString("Loc_LogScreenshotImageCopied"));
    }
}

