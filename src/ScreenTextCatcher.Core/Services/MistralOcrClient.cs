using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ScreenTextCatcher.Core.Models;

namespace ScreenTextCatcher.Core.Services;

public class MistralOcrClient : IMistralOcrClient
{
    private const string OcrEndpoint = "https://api.mistral.ai/v1/ocr";
    private readonly AppSettings _settings;
    private readonly HttpMessageHandler? _customHandler;

    public MistralOcrClient(AppSettings settings, HttpMessageHandler? customHandler = null)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _customHandler = customHandler;
    }

    public async Task<OcrResult> RecognizeTextAsync(
        byte[] imageBytes,
        string mimeType = "image/png",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.MistralApiKey))
        {
            return new OcrResult(false, string.Empty, "Mistral API-ключ не указан в настройках приложения.");
        }

        if (imageBytes == null || imageBytes.Length == 0)
        {
            return new OcrResult(false, string.Empty, "Передано пустое изображение для распознавания.");
        }

        var sw = Stopwatch.StartNew();

        try
        {
            var base64 = Convert.ToBase64String(imageBytes);
            var dataUrl = $"data:{mimeType};base64,{base64}";

            var requestBody = new
            {
                model = "mistral-ocr-latest",
                document = new
                {
                    type = "image_url",
                    image_url = dataUrl
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);

            using var handler = _customHandler ?? HttpHandlerFactory.CreateHandler(_settings.Proxy, TimeSpan.FromSeconds(15));
            using var httpClient = new HttpClient(handler, disposeHandler: _customHandler == null)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, OcrEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.MistralApiKey.Trim());
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            using var response = await httpClient.SendAsync(request, cancellationToken);
            sw.Stop();

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                var errorMsg = response.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => "Неверный API-ключ Mistral. Проверьте правильность ключа в настройках.",
                    HttpStatusCode.TooManyRequests => "Превышен лимит запросов к Mistral API. Попробуйте чуть позже.",
                    HttpStatusCode.Forbidden => "Доступ к Mistral API запрещен (HTTP 403 Forbidden).",
                    HttpStatusCode.ProxyAuthenticationRequired => "Прокси-сервер требует аутентификации (HTTP 407).",
                    _ => $"Ошибка сервиса Mistral OCR: HTTP {(int)response.StatusCode} ({errorBody})"
                };

                return new OcrResult(false, string.Empty, errorMsg, sw.ElapsedMilliseconds);
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var extractedText = ParseOcrResponse(responseJson);

            return new OcrResult(true, extractedText, null, sw.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            sw.Stop();
            return new OcrResult(false, string.Empty, "Распознавание отменено пользователем.", sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new OcrResult(false, string.Empty, $"Ошибка подключения к сети/Mistral API: {ex.Message}", sw.ElapsedMilliseconds);
        }
    }

    private static string ParseOcrResponse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // 1. Try Mistral /v1/ocr format: { "pages": [ { "markdown": "..." } ] }
        if (root.TryGetProperty("pages", out var pagesElement) && pagesElement.ValueKind == JsonValueKind.Array)
        {
            var sb = new StringBuilder();
            foreach (var page in pagesElement.EnumerateArray())
            {
                if (page.TryGetProperty("markdown", out var mdElement))
                {
                    var md = mdElement.GetString();
                    if (!string.IsNullOrWhiteSpace(md))
                    {
                        if (sb.Length > 0)
                        {
                            sb.AppendLine();
                        }
                        sb.Append(md);
                    }
                }
            }

            if (sb.Length > 0)
            {
                return sb.ToString().Trim();
            }
        }

        // 2. Try Chat Vision format fallback: { "choices": [ { "message": { "content": "..." } } ] }
        if (root.TryGetProperty("choices", out var choicesElement) && choicesElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var choice in choicesElement.EnumerateArray())
            {
                if (choice.TryGetProperty("message", out var msgElement) &&
                    msgElement.TryGetProperty("content", out var contentElement))
                {
                    var content = contentElement.GetString();
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        return content.Trim();
                    }
                }
            }
        }

        return string.Empty;
    }
}
