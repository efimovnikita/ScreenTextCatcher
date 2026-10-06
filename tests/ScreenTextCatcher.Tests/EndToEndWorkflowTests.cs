using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using Moq;
using Moq.Protected;
using ScreenTextCatcher.Core.Models;
using ScreenTextCatcher.Core.Services;

namespace ScreenTextCatcher.Tests;

public class EndToEndWorkflowTests : IDisposable
{
    private readonly string _tempDb;

    public EndToEndWorkflowTests()
    {
        var dir = Path.Combine(Path.GetTempPath(), "ScreenTextCatcher_E2E_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        _tempDb = Path.Combine(dir, "history.db");
    }

    public void Dispose()
    {
        var dir = Path.GetDirectoryName(_tempDb);
        if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
        {
            try { Directory.Delete(dir, true); } catch { }
        }
    }

    [Fact]
    public async Task FullWorkflow_CaptureToOcrToHistory_Succeeds()
    {
        // 1. Capture simulated screen in RAM
        var captureService = new ScreenCaptureService();
        using var sourceBitmap = new Bitmap(400, 200);
        using (var g = Graphics.FromImage(sourceBitmap))
        {
            g.Clear(Color.White);
            g.DrawString("Recognized Sample Text", new Font("Arial", 12), Brushes.Black, 10, 10);
        }

        var pngBytes = captureService.CropToPngBytes(sourceBitmap, new Rectangle(0, 0, 300, 100));
        pngBytes.Should().NotBeEmpty();

        // 2. Mock Mistral OCR response
        var ocrJson = """
        {
            "pages": [
                {
                    "index": 0,
                    "markdown": "Recognized Sample Text"
                }
            ]
        }
        """;

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(ocrJson)
            });

        var settings = new AppSettings { MistralApiKey = "mock_key" };
        var ocrClient = new MistralOcrClient(settings, mockHandler.Object);

        // 3. Send image to OCR
        var result = await ocrClient.RecognizeTextAsync(pngBytes);
        result.Success.Should().BeTrue();
        result.Text.Should().Be("Recognized Sample Text");

        // 4. Save to history
        var historyService = new HistoryService(_tempDb);
        historyService.Add(result.Text);

        historyService.Count().Should().Be(1);
        historyService.GetRecent().First().Text.Should().Be("Recognized Sample Text");
    }
}
