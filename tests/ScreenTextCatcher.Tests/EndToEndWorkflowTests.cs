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

    [Fact]
    public void FullWorkflow_ScreenshotWithAnnotations_SavesFile()
    {
        var tempFolder = Path.Combine(Path.GetTempPath(), "ScreenTextCatcher_E2E_Screenshots_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempFolder);

        try
        {
            byte[]? pngBytes = null;
            var thread = new Thread(() =>
            {
                var canvas = new System.Windows.Controls.Canvas
                {
                    Width = 300,
                    Height = 200,
                    ClipToBounds = true
                };

                var rect = Core.Helpers.AnnotationGeometryHelper.CreateRectangle(
                    new System.Windows.Point(20, 20),
                    new System.Windows.Point(100, 80));
                var arrow = Core.Helpers.AnnotationGeometryHelper.CreateArrow(
                    new System.Windows.Point(120, 30),
                    new System.Windows.Point(250, 150));

                canvas.Children.Add(rect);
                canvas.Children.Add(arrow);

                canvas.Measure(new System.Windows.Size(300, 200));
                canvas.Arrange(new System.Windows.Rect(0, 0, 300, 200));

                pngBytes = Core.Helpers.AnnotationExportHelper.RenderVisualToPng(canvas, 300, 200);

                var screenshotService = new ScreenshotService();
                var savedPath = screenshotService.SaveAndAccumulateClipboard(pngBytes, tempFolder, ClipboardDelimiter.NewLine);

                File.Exists(savedPath).Should().BeTrue();
                new FileInfo(savedPath).Length.Should().BeGreaterThan(0);
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            pngBytes.Should().NotBeNull();
            pngBytes!.Length.Should().BeGreaterThan(0);
        }
        finally
        {
            if (Directory.Exists(tempFolder))
            {
                try { Directory.Delete(tempFolder, true); } catch { }
            }
        }
    }

    [Fact]
    public void FullWorkflow_ScreenshotWithAnnotations_SaveAndCopyImage_CopiesImageDirectlyAndSavesFile()
    {
        var tempFolder = Path.Combine(Path.GetTempPath(), "ScreenTextCatcher_E2E_CopyImg_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempFolder);

        try
        {
            byte[]? pngBytes = null;
            byte[]? clipboardImageBytes = null;
            string? clipboardText = null;

            var thread = new Thread(() =>
            {
                var canvas = new System.Windows.Controls.Canvas
                {
                    Width = 300,
                    Height = 200,
                    ClipToBounds = true
                };

                var rect = Core.Helpers.AnnotationGeometryHelper.CreateRectangle(
                    new System.Windows.Point(10, 10),
                    new System.Windows.Point(80, 60));
                canvas.Children.Add(rect);

                canvas.Measure(new System.Windows.Size(300, 200));
                canvas.Arrange(new System.Windows.Rect(0, 0, 300, 200));

                pngBytes = Core.Helpers.AnnotationExportHelper.RenderVisualToPng(canvas, 300, 200);

                var screenshotService = new ScreenshotService(
                    imageClipboardSetter: bytes => clipboardImageBytes = bytes,
                    clipboardSetter: text => clipboardText = text);

                var savedPath = screenshotService.SaveAndCopyImageToClipboard(pngBytes, tempFolder);

                File.Exists(savedPath).Should().BeTrue();
                new FileInfo(savedPath).Length.Should().BeGreaterThan(0);
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            pngBytes.Should().NotBeNull();
            clipboardImageBytes.Should().NotBeNull();
            clipboardImageBytes.Should().Equal(pngBytes);
            clipboardText.Should().BeNull(); // Text paths must NOT be accumulated
        }
        finally
        {
            if (Directory.Exists(tempFolder))
            {
                try { Directory.Delete(tempFolder, true); } catch { }
            }
        }
    }
}

