namespace ScreenTextCatcher.Core.Services;

public interface IAutoStartService
{
    /// <summary>
    /// Gets whether autostart is currently enabled (shortcut exists in startup folder pointing to the target executable).
    /// </summary>
    bool IsAutoStartEnabled();

    /// <summary>
    /// Enables autostart by creating or updating the shortcut in the startup folder.
    /// </summary>
    bool EnableAutoStart();

    /// <summary>
    /// Disables autostart by removing the shortcut from the startup folder.
    /// </summary>
    bool DisableAutoStart();

    /// <summary>
    /// Sets autostart enabled or disabled based on the provided flag.
    /// </summary>
    bool SetAutoStart(bool enable);
}
