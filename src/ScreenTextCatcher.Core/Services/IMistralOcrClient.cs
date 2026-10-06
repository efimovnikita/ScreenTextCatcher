using ScreenTextCatcher.Core.Models;

namespace ScreenTextCatcher.Core.Services;

public interface IMistralOcrClient
{
    Task<OcrResult> RecognizeTextAsync(byte[] imageBytes, string mimeType = "image/png", CancellationToken cancellationToken = default);
}
