namespace ScreenTextCatcher.Core.Services;

public static class LocalizationManager
{
    public static string CurrentLanguage { get; private set; } = "ru";

    public static event Action? LanguageChanged;

    private static readonly Dictionary<string, string> RuDictionary = new()
    {
        // MainWindow
        ["Loc_MainWindowTitle"] = "ScreenTextCatcher — Панель управления и диагностики",
        ["Loc_MainHeaderTitle"] = "ScreenTextCatcher — Панель управления и диагностики",
        ["Loc_MainHeaderSubtitle"] = "Настройка Mistral API, прокси-сервера, SQLite истории и просмотр журнала Serilog",
        ["Loc_SettingsCardTitle"] = "⚙ Настройки Mistral API и Прокси",
        ["Loc_ApiKeyLabel"] = "Mistral API Key:",
        ["Loc_ApiKeyTooltip"] = "Введите ваш API ключ Mistral (начинается с ...)",
        ["Loc_UseProxy"] = "Использовать прокси",
        ["Loc_ProxyType"] = "Тип:",
        ["Loc_ProxyHost"] = "Хост прокси:",
        ["Loc_ProxyPort"] = "Порт:",
        ["Loc_ProxyUser"] = "Логин (если есть):",
        ["Loc_ProxyPassword"] = "Пароль:",
        ["Loc_SaveSettings"] = "💾 Сохранить настройки",
        ["Loc_SqliteCardTitle"] = "🗄 База данных SQLite",
        ["Loc_SqliteDescription"] = "Последние распознавания сохраняются в history.db и будут доступны в меню трея (до 100 записей).",
        ["Loc_LogStreamTitle"] = "📋 Журнал событий (Serilog Live Stream):",
        ["Loc_BtnCapture"] = "✂ Активировать оверлей захвата",
        ["Loc_BtnSettings"] = "⚙ Настройки",
        ["Loc_BtnHistory"] = "🗄 История",
        ["Loc_BtnLogs"] = "📋 Журнал логов",
        ["Loc_BtnTestPing"] = "🌐 Проверить соединение (Test/Ping)",
        ["Loc_BtnTestOcr"] = "🔍 Тест OCR (Sample)",
        ["Loc_BtnTestHistory"] = "➕ Тестовая запись в историю",
        ["Loc_BtnTestLog"] = "📝 Тестовый лог",
        ["Loc_BtnOpenFolder"] = "📂 Папка AppData",
        ["Loc_HistoryCountFormat"] = "Записей в истории: {0} / 100",

        // Settings Window
        ["Loc_SettingsTitle"] = "Настройки — ScreenTextCatcher",
        ["Loc_SettingsHeader"] = "⚙ Настройки приложения",
        ["Loc_MistralApiGroup"] = "Ключ Mistral OCR API",
        ["Loc_MistralApiHelp"] = "Ключ используется для отправки запросов в облачный сервис Mistral OCR.",
        ["Loc_ProxyGroup"] = "Прокси-сервер",
        ["Loc_HotkeyGroup"] = "Глобальная горячая клавиша",
        ["Loc_KeyLabel"] = "Клавиша:",
        ["Loc_HotkeyHelp"] = "По умолчанию: Win+Shift+X.",
        ["Loc_InterfaceAndSoundGroup"] = "Интерфейс и звук",
        ["Loc_SoundFeedback"] = "Тихий звуковой сигнал при готовности текста",
        ["Loc_SystemGroup"] = "Система",
        ["Loc_AutoStart"] = "Запускать при старте Windows",
        ["Loc_AutoStartHelp"] = "Автоматический запуск ScreenTextCatcher в системный трей при входе в систему.",
        ["Loc_AppLanguage"] = "Язык интерфейса:",
        ["Loc_BtnSave"] = "💾 Сохранить",
        ["Loc_BtnCancel"] = "Отмена",
        ["Loc_TestingConnection"] = "Проверка соединения...",
        ["Loc_ProxyError"] = "Ошибка прокси: {0}",
        ["Loc_TestSuccess"] = "Успешно! {0}",
        ["Loc_TestWarning"] = "Внимание: {0}",

        // History Window
        ["Loc_HistoryTitle"] = "История распознаваний — ScreenTextCatcher",
        ["Loc_HistoryHeader"] = "📋 История последних распознаваний (до 100 записей)",
        ["Loc_SearchLabel"] = "🔍 Поиск:",
        ["Loc_BtnCopySelected"] = "📋 Скопировать выбранное (Ctrl+C)",
        ["Loc_BtnClearHistory"] = "🧹 Очистить историю",
        ["Loc_BtnClose"] = "Закрыть",
        ["Loc_HistoryCopied"] = "Фрагмент скопирован в буфер обмена!",
        ["Loc_ClearHistoryConfirm"] = "Вы уверены, что хотите удалить всю историю распознаваний?",
        ["Loc_ClearHistoryTitle"] = "Очистка истории",

        // Log Viewer Window
        ["Loc_LogTitle"] = "Журнал логов — ScreenTextCatcher",
        ["Loc_LogHeader"] = "📝 Журнал событий и диагностика",
        ["Loc_BtnCopyLogs"] = "📋 Скопировать всё",
        ["Loc_BtnOpenFolderLogs"] = "📂 Открыть папку с логами",
        ["Loc_BtnClearLogs"] = "🧹 Очистить экран",
        ["Loc_LogsCopied"] = "Содержимое журнала скопировано в буфер обмена.",

        // Overlay Window
        ["Loc_OverlayHint"] = "Выделите область экрана рамкой. Нажмите Esc или кликните правой кнопкой мыши для отмены.",
        ["Loc_AnnotateToolbarRect"] = "Рамка",
        ["Loc_AnnotateToolbarArrow"] = "Стрелка",
        ["Loc_AnnotateToolbarUndo"] = "Отменить действие (Ctrl+Z)",
        ["Loc_AnnotateToolbarCopy"] = "Копировать",
        ["Loc_AnnotateToolbarCopyTooltip"] = "Скопировать изображение в буфер обмена и сохранить на диск",
        ["Loc_AnnotateToolbarDone"] = "Сохранить (Enter)",
        ["Loc_AnnotateToolbarCancel"] = "Отмена (Esc)",

        // App Mode & Screenshots
        ["Loc_ModeGroup"] = "Режим работы приложения",
        ["Loc_ModeOcr"] = "Распознавание текста (Mistral OCR)",
        ["Loc_ModeScreenshot"] = "Сохранение скриншотов в папку",
        ["Loc_ScreenshotFolderLabel"] = "Папка для скриншотов:",
        ["Loc_BtnBrowse"] = "Обзор...",
        ["Loc_BtnOpenScreenshotFolder"] = "📂 Открыть папку скриншотов",
        ["Loc_LogScreenshotSaved"] = "Скриншот сохранён: {0}",
        ["Loc_LogScreenshotImageCopied"] = "Скриншот сохранён на диск и скопирован в буфер обмена как изображение: {0}",
        ["Loc_NotificationScreenshotErrorTitle"] = "Ошибка сохранения скриншота",
        ["Loc_NotificationScreenshotErrorMsg"] = "Не удалось сохранить скриншот: {0}",
        ["Loc_ClipboardDelimiterLabel"] = "Разделитель путей в буфере:",
        ["Loc_DelimiterNewLine"] = "Перенос строки (\\r\\n)",
        ["Loc_DelimiterSpace"] = "Пробел (' ')",

        // Tray Menu & Notifications
        ["Loc_TrayCapture"] = "✂ Захват текста экрана (Win+Shift+X)",
        ["Loc_TrayCaptureScreenshot"] = "✂ Сделать скриншот (Win+Shift+X)",
        ["Loc_TrayModeOcr"] = "Режим: Распознавание текста (OCR)",
        ["Loc_TrayModeScreenshot"] = "Режим: Сохранение скриншотов",
        ["Loc_TrayRecent"] = "📋 Последние распознавания",
        ["Loc_TrayAllHistory"] = "📋 Вся история (100 записей)...",
        ["Loc_TraySettings"] = "⚙ Настройки...",
        ["Loc_TrayAutoStart"] = "🚀 Автозапуск при старте Windows",
        ["Loc_TrayLogs"] = "📝 Журнал логов...",
        ["Loc_TrayShowDashboard"] = "🖥 Открыть панель управления",
        ["Loc_TrayExit"] = "❌ Выход",
        ["Loc_TrayEmptyHistory"] = "(История пуста)",
        ["Loc_TraySelectArea"] = "Выделите область...",
        ["Loc_TrayRecognizing"] = "Распознавание Mistral OCR...",
        ["Loc_TrayNoApiKey"] = "Не указан API-ключ!",
        ["Loc_TrayOcrError"] = "Ошибка OCR",
        ["Loc_TrayCaptureFail"] = "Сбой захвата",
        ["Loc_NotificationErrorTitle"] = "Ошибка ScreenTextCatcher",
        ["Loc_NotificationErrorKey"] = "Укажите Mistral API ключ в настройках.",
        ["Loc_NotificationOcrErrorTitle"] = "Ошибка распознавания"
    };

    private static readonly Dictionary<string, string> EnDictionary = new()
    {
        // MainWindow
        ["Loc_MainWindowTitle"] = "ScreenTextCatcher — Control Panel & Diagnostics",
        ["Loc_MainHeaderTitle"] = "ScreenTextCatcher — Control Panel & Diagnostics",
        ["Loc_MainHeaderSubtitle"] = "Mistral API configuration, proxy server, SQLite history and Serilog live stream",
        ["Loc_SettingsCardTitle"] = "⚙ Mistral API & Proxy Settings",
        ["Loc_ApiKeyLabel"] = "Mistral API Key:",
        ["Loc_ApiKeyTooltip"] = "Enter your Mistral API key (starts with ...)",
        ["Loc_UseProxy"] = "Use proxy",
        ["Loc_ProxyType"] = "Type:",
        ["Loc_ProxyHost"] = "Proxy host:",
        ["Loc_ProxyPort"] = "Port:",
        ["Loc_ProxyUser"] = "Username (optional):",
        ["Loc_ProxyPassword"] = "Password:",
        ["Loc_SaveSettings"] = "💾 Save Settings",
        ["Loc_SqliteCardTitle"] = "🗄 SQLite Database",
        ["Loc_SqliteDescription"] = "Recent recognitions are saved in history.db and available in the tray menu (up to 100 entries).",
        ["Loc_LogStreamTitle"] = "📋 Event Log (Serilog Live Stream):",
        ["Loc_BtnCapture"] = "✂ Capture Screen",
        ["Loc_BtnSettings"] = "⚙ Settings",
        ["Loc_BtnHistory"] = "🗄 History",
        ["Loc_BtnLogs"] = "📋 Event Log",
        ["Loc_BtnTestPing"] = "🌐 Test Connection (Test/Ping)",
        ["Loc_BtnTestOcr"] = "🔍 Test OCR (Sample)",
        ["Loc_BtnTestHistory"] = "➕ Add Test History Entry",
        ["Loc_BtnTestLog"] = "📝 Test Log Entry",
        ["Loc_BtnOpenFolder"] = "📂 AppData Folder",
        ["Loc_HistoryCountFormat"] = "History records: {0} / 100",

        // Settings Window
        ["Loc_SettingsTitle"] = "Settings — ScreenTextCatcher",
        ["Loc_SettingsHeader"] = "⚙ Application Settings",
        ["Loc_MistralApiGroup"] = "Mistral OCR API Key",
        ["Loc_MistralApiHelp"] = "The key is used to send requests to the Mistral OCR cloud service.",
        ["Loc_ProxyGroup"] = "Proxy Server",
        ["Loc_HotkeyGroup"] = "Global Capture Hotkey",
        ["Loc_KeyLabel"] = "Key:",
        ["Loc_HotkeyHelp"] = "Default: Win+Shift+X.",
        ["Loc_InterfaceAndSoundGroup"] = "Interface & Sound",
        ["Loc_SoundFeedback"] = "Play subtle sound when text is copied",
        ["Loc_SystemGroup"] = "System",
        ["Loc_AutoStart"] = "Start with Windows",
        ["Loc_AutoStartHelp"] = "Automatically start ScreenTextCatcher in the system tray when logging in.",
        ["Loc_AppLanguage"] = "UI Language:",
        ["Loc_BtnSave"] = "💾 Save",
        ["Loc_BtnCancel"] = "Cancel",
        ["Loc_TestingConnection"] = "Testing connection...",
        ["Loc_ProxyError"] = "Proxy error: {0}",
        ["Loc_TestSuccess"] = "Success! {0}",
        ["Loc_TestWarning"] = "Warning: {0}",

        // History Window
        ["Loc_HistoryTitle"] = "Recognition History — ScreenTextCatcher",
        ["Loc_HistoryHeader"] = "📋 Recent Recognitions History (up to 100 entries)",
        ["Loc_SearchLabel"] = "🔍 Search:",
        ["Loc_BtnCopySelected"] = "📋 Copy Selected (Ctrl+C)",
        ["Loc_BtnClearHistory"] = "🧹 Clear History",
        ["Loc_BtnClose"] = "Close",
        ["Loc_HistoryCopied"] = "Snippet copied to clipboard!",
        ["Loc_ClearHistoryConfirm"] = "Are you sure you want to delete all recognition history?",
        ["Loc_ClearHistoryTitle"] = "Clear History",

        // Log Viewer Window
        ["Loc_LogTitle"] = "Event Log — ScreenTextCatcher",
        ["Loc_LogHeader"] = "📝 Event Log & Diagnostics",
        ["Loc_BtnCopyLogs"] = "📋 Copy All",
        ["Loc_BtnOpenFolderLogs"] = "📂 Open Log Folder",
        ["Loc_BtnClearLogs"] = "🧹 Clear Display",
        ["Loc_LogsCopied"] = "Log contents copied to clipboard.",

        // Overlay Window
        ["Loc_OverlayHint"] = "Select screen area with mouse. Press Esc or right-click to cancel.",
        ["Loc_AnnotateToolbarRect"] = "Rectangle",
        ["Loc_AnnotateToolbarArrow"] = "Arrow",
        ["Loc_AnnotateToolbarUndo"] = "Undo (Ctrl+Z)",
        ["Loc_AnnotateToolbarCopy"] = "Copy",
        ["Loc_AnnotateToolbarCopyTooltip"] = "Copy image to clipboard and save to disk",
        ["Loc_AnnotateToolbarDone"] = "Save (Enter)",
        ["Loc_AnnotateToolbarCancel"] = "Cancel (Esc)",

        // App Mode & Screenshots
        ["Loc_ModeGroup"] = "Application Mode",
        ["Loc_ModeOcr"] = "Text Recognition (Mistral OCR)",
        ["Loc_ModeScreenshot"] = "Save Screenshots to Folder",
        ["Loc_ScreenshotFolderLabel"] = "Screenshots folder:",
        ["Loc_BtnBrowse"] = "Browse...",
        ["Loc_BtnOpenScreenshotFolder"] = "📂 Open Screenshots Folder",
        ["Loc_LogScreenshotSaved"] = "Screenshot saved: {0}",
        ["Loc_LogScreenshotImageCopied"] = "Screenshot saved to disk and copied to clipboard as image: {0}",
        ["Loc_NotificationScreenshotErrorTitle"] = "Screenshot Save Error",
        ["Loc_NotificationScreenshotErrorMsg"] = "Failed to save screenshot: {0}",
        ["Loc_ClipboardDelimiterLabel"] = "Clipboard path delimiter:",
        ["Loc_DelimiterNewLine"] = "New line (\\r\\n)",
        ["Loc_DelimiterSpace"] = "Space (' ')",

        // Tray Menu & Notifications
        ["Loc_TrayCapture"] = "✂ Capture Screen Text (Win+Shift+X)",
        ["Loc_TrayCaptureScreenshot"] = "✂ Take Screenshot (Win+Shift+X)",
        ["Loc_TrayModeOcr"] = "Mode: Text Recognition (OCR)",
        ["Loc_TrayModeScreenshot"] = "Mode: Save Screenshots",
        ["Loc_TrayRecent"] = "📋 Recent Recognitions",
        ["Loc_TrayAllHistory"] = "📋 Full History (100 items)...",
        ["Loc_TraySettings"] = "⚙ Settings...",
        ["Loc_TrayAutoStart"] = "🚀 Start with Windows",
        ["Loc_TrayLogs"] = "📝 Event Log...",
        ["Loc_TrayShowDashboard"] = "🖥 Open Dashboard",
        ["Loc_TrayExit"] = "❌ Exit",
        ["Loc_TrayEmptyHistory"] = "(History is empty)",
        ["Loc_TraySelectArea"] = "Select region...",
        ["Loc_TrayRecognizing"] = "Recognizing via Mistral OCR...",
        ["Loc_TrayNoApiKey"] = "API key not set!",
        ["Loc_TrayOcrError"] = "OCR Error",
        ["Loc_TrayCaptureFail"] = "Capture failure",
        ["Loc_NotificationErrorTitle"] = "ScreenTextCatcher Error",
        ["Loc_NotificationErrorKey"] = "Please specify Mistral API key in Settings.",
        ["Loc_NotificationOcrErrorTitle"] = "Recognition Error"
    };

    public static void SetLanguage(string lang)
    {
        CurrentLanguage = string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase) ? "en" : "ru";
        var dict = CurrentLanguage == "en" ? EnDictionary : RuDictionary;

        if (System.Windows.Application.Current != null)
        {
            foreach (var kvp in dict)
            {
                System.Windows.Application.Current.Resources[kvp.Key] = kvp.Value;
            }
        }

        LanguageChanged?.Invoke();
    }

    public static string GetString(string key, string fallback = "")
    {
        var dict = CurrentLanguage == "en" ? EnDictionary : RuDictionary;
        if (dict.TryGetValue(key, out var val))
        {
            return val;
        }

        if (RuDictionary.TryGetValue(key, out var ruVal))
        {
            return ruVal;
        }

        return string.IsNullOrEmpty(fallback) ? key : fallback;
    }
}
