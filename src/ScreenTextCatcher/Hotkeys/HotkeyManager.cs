using System.Runtime.InteropServices;
using System.Windows.Interop;
using ScreenTextCatcher.Core.Logging;
using ScreenTextCatcher.Core.Models;
using Serilog;

namespace ScreenTextCatcher.Hotkeys;

public class HotkeyManager : IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private const int HotkeyId = 9001;

    private const uint MOD_ALT = 0x0001;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_WIN = 0x0008;
    private const uint MOD_NOREPEAT = 0x4000;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private IntPtr _windowHandle;
    private HwndSource? _hwndSource;
    private Action? _onHotkeyPressed;
    private bool _isRegistered = false;

    public bool IsRegistered => _isRegistered;

    public bool Initialize(IntPtr windowHandle)
    {
        _windowHandle = windowHandle;
        _hwndSource = HwndSource.FromHwnd(_windowHandle);
        _hwndSource?.AddHook(HwndHook);
        return true;
    }

    public bool Register(HotkeySettings settings, Action onHotkeyPressed)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(onHotkeyPressed);

        Unregister();

        _onHotkeyPressed = onHotkeyPressed;

        if (_windowHandle == IntPtr.Zero)
        {
            Log.Warning("HotkeyManager: Handle окна равен нулю, регистрация отложена.");
            return false;
        }

        uint modifiers = MOD_NOREPEAT;
        if (settings.Alt) modifiers |= MOD_ALT;
        if (settings.Ctrl) modifiers |= MOD_CONTROL;
        if (settings.Shift) modifiers |= MOD_SHIFT;
        if (settings.Win) modifiers |= MOD_WIN;

        uint vk = ConvertKeyToVk(settings.Key);
        if (vk == 0)
        {
            Log.Error("HotkeyManager: Не удалось преобразовать клавишу '{Key}' в Virtual Key код.", settings.Key);
            return false;
        }

        bool result = RegisterHotKey(_windowHandle, HotkeyId, modifiers, vk);
        if (result)
        {
            _isRegistered = true;
            Log.Information("Глобальный хоткей успешно зарегистрирован: {Mod}+{Key}", FormatHotkey(settings), settings.Key);
        }
        else
        {
            int error = Marshal.GetLastWin32Error();
            Log.Error("Не удалось зарегистрировать глобальный хоткей: Win32 Error {Code}. Возможно, комбинация уже занята другой программой.", error);
        }

        return result;
    }

    public void Unregister()
    {
        if (_isRegistered && _windowHandle != IntPtr.Zero)
        {
            UnregisterHotKey(_windowHandle, HotkeyId);
            _isRegistered = false;
            Log.Information("Глобальный хоткей разрегистрирован.");
        }
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY && wParam.ToInt32() == HotkeyId)
        {
            handled = true;
            Log.Information("Сработало нажатие глобального хоткея!");
            _onHotkeyPressed?.Invoke();
        }

        return IntPtr.Zero;
    }

    private static uint ConvertKeyToVk(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return 0x58; // 'X'
        }

        key = key.Trim().ToUpperInvariant();

        if (key.Length == 1 && key[0] >= 'A' && key[0] <= 'Z')
        {
            return (uint)key[0];
        }

        if (key.Length == 1 && key[0] >= '0' && key[0] <= '9')
        {
            return (uint)key[0];
        }

        return key switch
        {
            "F1" => 0x70,
            "F2" => 0x71,
            "F3" => 0x72,
            "F4" => 0x73,
            "F5" => 0x74,
            "F6" => 0x75,
            "F7" => 0x76,
            "F8" => 0x77,
            "F9" => 0x78,
            "F10" => 0x79,
            "F11" => 0x7A,
            "F12" => 0x7B,
            "SPACE" => 0x20,
            _ => (uint)key[0]
        };
    }

    private static string FormatHotkey(HotkeySettings s)
    {
        var parts = new List<string>();
        if (s.Win) parts.Add("Win");
        if (s.Ctrl) parts.Add("Ctrl");
        if (s.Alt) parts.Add("Alt");
        if (s.Shift) parts.Add("Shift");
        return string.Join("+", parts);
    }

    public void Dispose()
    {
        Unregister();
        if (_hwndSource != null)
        {
            _hwndSource.RemoveHook(HwndHook);
            _hwndSource = null;
        }
    }
}
