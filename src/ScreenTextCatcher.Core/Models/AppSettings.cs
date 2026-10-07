namespace ScreenTextCatcher.Core.Models;

public enum ProxyType
{
    Http,
    Https,
    Socks5
}

public class ProxySettings
{
    public bool Enabled { get; set; } = false;
    public ProxyType Type { get; set; } = ProxyType.Http;
    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 8080;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class HotkeySettings
{
    public bool Win { get; set; } = true;
    public bool Shift { get; set; } = true;
    public bool Ctrl { get; set; } = false;
    public bool Alt { get; set; } = false;
    public string Key { get; set; } = "X";
}

public class AppSettings
{
    public string MistralApiKey { get; set; } = string.Empty;
    public ProxySettings Proxy { get; set; } = new();
    public HotkeySettings Hotkey { get; set; } = new();
    public bool SoundFeedback { get; set; } = true;
    public string Language { get; set; } = "ru";
    public bool AutoStart { get; set; } = false;
}
