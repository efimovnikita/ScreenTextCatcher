using ScreenTextCatcher.Core.Models;

namespace ScreenTextCatcher.Core.Services;

public interface IScreenshotService
{
    string SaveScreenshot(byte[] pngBytes, string folderPath);
    string CalculateUpdatedClipboardText(string? currentClipboardText, string newFilePath, string folderPath, ClipboardDelimiter delimiter = ClipboardDelimiter.NewLine);
    string SaveAndAccumulateClipboard(byte[] pngBytes, string folderPath, ClipboardDelimiter delimiter = ClipboardDelimiter.NewLine);
}
