using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Serilog;

namespace ScreenTextCatcher.Core.Services;

public class AutoStartService : IAutoStartService
{
    private const string DefaultShortcutName = "ScreenTextCatcher.lnk";
    private readonly string _startupFolderPath;
    private readonly string _targetExecutablePath;
    private readonly string _shortcutPath;

    public AutoStartService(
        string? startupFolderPath = null,
        string? targetExecutablePath = null,
        string shortcutName = DefaultShortcutName)
    {
        _startupFolderPath = string.IsNullOrWhiteSpace(startupFolderPath)
            ? Environment.GetFolderPath(Environment.SpecialFolder.Startup)
            : startupFolderPath;

        _targetExecutablePath = string.IsNullOrWhiteSpace(targetExecutablePath)
            ? (Environment.ProcessPath ?? string.Empty)
            : targetExecutablePath;

        _shortcutPath = Path.Combine(_startupFolderPath, shortcutName);
    }

    public string ShortcutPath => _shortcutPath;
    public string TargetExecutablePath => _targetExecutablePath;

    public bool IsAutoStartEnabled()
    {
        try
        {
            if (!File.Exists(_shortcutPath))
            {
                return false;
            }

            var target = GetShortcutTarget(_shortcutPath);
            if (string.IsNullOrWhiteSpace(target))
            {
                return false;
            }

            return string.Equals(
                Path.GetFullPath(target),
                Path.GetFullPath(_targetExecutablePath),
                StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to check autostart status for shortcut {ShortcutPath}", _shortcutPath);
            return false;
        }
    }

    public bool EnableAutoStart()
    {
        try
        {
            if (!Directory.Exists(_startupFolderPath))
            {
                Directory.CreateDirectory(_startupFolderPath);
            }

            CreateOrUpdateShortcut(_shortcutPath, _targetExecutablePath);
            Log.Information("Autostart shortcut successfully created or updated at {ShortcutPath}", _shortcutPath);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create autostart shortcut at {ShortcutPath}", _shortcutPath);
            return false;
        }
    }

    public bool DisableAutoStart()
    {
        try
        {
            if (File.Exists(_shortcutPath))
            {
                File.Delete(_shortcutPath);
                Log.Information("Autostart shortcut deleted at {ShortcutPath}", _shortcutPath);
            }

            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to delete autostart shortcut at {ShortcutPath}", _shortcutPath);
            return false;
        }
    }

    public bool SetAutoStart(bool enable)
    {
        return enable ? EnableAutoStart() : DisableAutoStart();
    }

    private static void CreateOrUpdateShortcut(string shortcutPath, string targetPath)
    {
        var link = (IShellLinkW)new ShellLink();
        link.SetPath(targetPath);
        var workingDir = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrEmpty(workingDir))
        {
            link.SetWorkingDirectory(workingDir);
        }

        link.SetDescription("ScreenTextCatcher Autostart");
        var persistFile = (IPersistFile)link;
        persistFile.Save(shortcutPath, true);
    }

    private static string GetShortcutTarget(string shortcutPath)
    {
        var link = (IShellLinkW)new ShellLink();
        var persistFile = (IPersistFile)link;
        persistFile.Load(shortcutPath, 0);

        var targetBuilder = new StringBuilder(260);
        link.GetPath(targetBuilder, targetBuilder.Capacity, IntPtr.Zero, 0);
        return targetBuilder.ToString();
    }

    [ComImport]
    [Guid("00021401-0000-0000-C000-000000000046")]
    private class ShellLink
    {
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    private interface IShellLinkW
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, IntPtr pfd, uint fFlags);
        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotkey(out ushort pwHotkey);
        void SetHotkey(ushort wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
        void Resolve(IntPtr hwnd, uint fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("0000010b-0000-0000-C000-000000000046")]
    private interface IPersistFile
    {
        void GetClassID(out Guid pClassID);
        [PreserveSig]
        int IsDirty();
        void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);
        void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fRemember);
        void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
        void GetCurFile([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder ppszFileName);
    }
}
