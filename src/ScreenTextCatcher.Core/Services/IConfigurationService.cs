using ScreenTextCatcher.Core.Models;

namespace ScreenTextCatcher.Core.Services;

public interface IConfigurationService
{
    AppSettings CurrentSettings { get; }
    string SettingsFilePath { get; }
    AppSettings Load();
    void Save(AppSettings settings);
}
