using System.IO;
using System.Text.Json;
using ScreenTextCatcher.Core.Models;

namespace ScreenTextCatcher.Core.Services;

public class ConfigurationService : IConfigurationService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly object _lock = new();
    private readonly string _settingsFilePath;
    private AppSettings _currentSettings;

    public ConfigurationService(string? settingsFilePath = null)
    {
        if (string.IsNullOrWhiteSpace(settingsFilePath))
        {
            var appDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ScreenTextCatcher");
            _settingsFilePath = Path.Combine(appDataDir, "settings.json");
        }
        else
        {
            _settingsFilePath = settingsFilePath;
        }

        _currentSettings = Load();
    }

    public string SettingsFilePath => _settingsFilePath;

    public AppSettings CurrentSettings
    {
        get
        {
            lock (_lock)
            {
                return _currentSettings;
            }
        }
    }

    public AppSettings Load()
    {
        lock (_lock)
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    var deserialized = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                    if (deserialized != null)
                    {
                        _currentSettings = deserialized;
                        return _currentSettings;
                    }
                }
            }
            catch
            {
                // Fall back to default settings on read error or corrupted file
            }

            _currentSettings = new AppSettings();
            Save(_currentSettings);
            return _currentSettings;
        }
    }

    public void Save(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        lock (_lock)
        {
            _currentSettings = settings;
            var directory = Path.GetDirectoryName(_settingsFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(_settingsFilePath, json);
        }
    }
}
