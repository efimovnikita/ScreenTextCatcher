using System.IO;
using System.Threading;
using System.Windows;
using ScreenTextCatcher.Core.Models;
using Serilog;

namespace ScreenTextCatcher.Core.Services;

public class ScreenshotService : IScreenshotService
{
    private readonly Func<DateTime> _timeProvider;
    private readonly Action<string> _clipboardSetter;
    private readonly Func<string?> _clipboardGetter;

    public ScreenshotService(
        Func<DateTime>? timeProvider = null,
        Action<string>? clipboardSetter = null,
        Func<string?>? clipboardGetter = null)
    {
        _timeProvider = timeProvider ?? (() => DateTime.Now);
        _clipboardSetter = clipboardSetter ?? SetClipboardWithRetry;
        _clipboardGetter = clipboardGetter ?? GetClipboardTextSafe;
    }

    public string SaveScreenshot(byte[] pngBytes, string folderPath)
    {
        ArgumentNullException.ThrowIfNull(pngBytes);
        ArgumentException.ThrowIfNullOrWhiteSpace(folderPath);

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var timestamp = _timeProvider().ToString("yyyyMMdd_HHmmss");
        var baseFileName = $"Screenshot_{timestamp}.png";
        var targetFilePath = Path.Combine(folderPath, baseFileName);

        if (!File.Exists(targetFilePath))
        {
            File.WriteAllBytes(targetFilePath, pngBytes);
            return targetFilePath;
        }

        int counter = 1;
        while (true)
        {
            var collisionFileName = $"Screenshot_{timestamp}_{counter}.png";
            targetFilePath = Path.Combine(folderPath, collisionFileName);
            if (!File.Exists(targetFilePath))
            {
                File.WriteAllBytes(targetFilePath, pngBytes);
                return targetFilePath;
            }
            counter++;
        }
    }

    public string CalculateUpdatedClipboardText(
        string? currentClipboardText,
        string newFilePath,
        string folderPath,
        ClipboardDelimiter delimiter = ClipboardDelimiter.NewLine)
    {
        if (string.IsNullOrWhiteSpace(currentClipboardText))
        {
            return newFilePath;
        }

        var fullFolderPath = Path.GetFullPath(folderPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        List<string> rawTokens;

        if (delimiter == ClipboardDelimiter.Space)
        {
            rawTokens = ExtractPaths(currentClipboardText, fullFolderPath);
        }
        else
        {
            rawTokens = currentClipboardText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrEmpty(line))
                .ToList();
        }

        if (rawTokens.Count == 0)
        {
            return newFilePath;
        }

        var validExistingPaths = new List<string>();

        foreach (var line in rawTokens)
        {
            try
            {
                var fullPath = Path.GetFullPath(line);
                var dir = Path.GetDirectoryName(fullPath)?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                if (dir == null || (!dir.Equals(fullFolderPath, StringComparison.OrdinalIgnoreCase) &&
                    !dir.StartsWith(fullFolderPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)))
                {
                    return newFilePath;
                }

                if (!File.Exists(fullPath))
                {
                    return newFilePath;
                }

                validExistingPaths.Add(fullPath);
            }
            catch
            {
                return newFilePath;
            }
        }

        validExistingPaths.Add(Path.GetFullPath(newFilePath));
        var separator = delimiter == ClipboardDelimiter.Space ? " " : Environment.NewLine;
        return string.Join(separator, validExistingPaths);
    }

    private static List<string> ExtractPaths(string text, string fullFolderPath)
    {
        if (text.Contains('\n') || text.Contains('\r'))
        {
            return text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToList();
        }

        if (!fullFolderPath.Contains(' '))
        {
            return text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .ToList();
        }

        var result = new List<string>();
        int searchStart = 0;
        while (searchStart < text.Length)
        {
            int folderIdx = text.IndexOf(fullFolderPath, searchStart, StringComparison.OrdinalIgnoreCase);
            if (folderIdx < 0)
            {
                result.Add(text.Substring(searchStart).Trim());
                break;
            }

            int nextSpace = text.IndexOf(' ', folderIdx + fullFolderPath.Length);
            if (nextSpace < 0)
            {
                result.Add(text.Substring(folderIdx).Trim());
                break;
            }

            result.Add(text.Substring(folderIdx, nextSpace - folderIdx).Trim());
            searchStart = nextSpace + 1;
        }

        return result.Where(r => !string.IsNullOrEmpty(r)).ToList();
    }

    public string SaveAndAccumulateClipboard(byte[] pngBytes, string folderPath, ClipboardDelimiter delimiter = ClipboardDelimiter.NewLine)
    {
        var savedFilePath = SaveScreenshot(pngBytes, folderPath);
        var currentText = _clipboardGetter();
        var updatedText = CalculateUpdatedClipboardText(currentText, savedFilePath, folderPath, delimiter);
        _clipboardSetter(updatedText);
        return savedFilePath;
    }

    private static void SetClipboardWithRetry(string text)
    {
        for (int i = 0; i < 5; i++)
        {
            try
            {
                Clipboard.SetText(text);
                return;
            }
            catch (Exception ex)
            {
                if (i == 4)
                {
                    Log.Warning(ex, "Failed to set clipboard text after 5 attempts.");
                    throw;
                }
                Thread.Sleep(50);
            }
        }
    }

    private static string? GetClipboardTextSafe()
    {
        for (int i = 0; i < 3; i++)
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    return Clipboard.GetText();
                }
                return null;
            }
            catch (Exception ex)
            {
                if (i == 2)
                {
                    Log.Debug(ex, "Failed to read clipboard text.");
                    return null;
                }
                Thread.Sleep(30);
            }
        }
        return null;
    }
}
