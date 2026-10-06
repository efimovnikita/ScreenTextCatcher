namespace ScreenTextCatcher.Core.Models;

public record OcrResult(
    bool Success,
    string Text,
    string? ErrorMessage = null,
    long ElapsedMilliseconds = 0
);
