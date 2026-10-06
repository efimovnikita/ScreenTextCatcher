using System.Net;
using System.Net.Http;
using FluentAssertions;
using Moq;
using Moq.Protected;
using ScreenTextCatcher.Core.Models;
using ScreenTextCatcher.Core.Services;

namespace ScreenTextCatcher.Tests;

public class MistralOcrClientTests
{
    private readonly AppSettings _defaultSettings = new()
    {
        MistralApiKey = "test_valid_key",
        Proxy = new ProxySettings { Enabled = false }
    };

    [Fact]
    public async Task RecognizeTextAsync_WhenApiKeyMissing_ReturnsFailure()
    {
        var settings = new AppSettings { MistralApiKey = "" };
        var client = new MistralOcrClient(settings);

        var result = await client.RecognizeTextAsync(new byte[] { 1, 2, 3 });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("API-ключ");
    }

    [Fact]
    public async Task RecognizeTextAsync_WhenImageBytesEmpty_ReturnsFailure()
    {
        var client = new MistralOcrClient(_defaultSettings);

        var result = await client.RecognizeTextAsync(Array.Empty<byte>());

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("пустое");
    }

    [Fact]
    public async Task RecognizeTextAsync_WhenOcrEndpointSucceeds_ExtractsMarkdown()
    {
        var jsonResponse = """
        {
            "pages": [
                {
                    "index": 0,
                    "markdown": "# Header\nSome recognized text line 1\nLine 2"
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
                Content = new StringContent(jsonResponse)
            });

        var client = new MistralOcrClient(_defaultSettings, mockHandler.Object);
        var result = await client.RecognizeTextAsync(new byte[] { 0x89, 0x50, 0x4E, 0x47 });

        result.Success.Should().BeTrue();
        result.Text.Should().Contain("Some recognized text line 1");
    }

    [Fact]
    public async Task RecognizeTextAsync_WhenVisionChatFormatReturned_ExtractsChoicesContent()
    {
        var jsonResponse = """
        {
            "choices": [
                {
                    "message": {
                        "content": "Vision extracted snippet"
                    }
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
                Content = new StringContent(jsonResponse)
            });

        var client = new MistralOcrClient(_defaultSettings, mockHandler.Object);
        var result = await client.RecognizeTextAsync(new byte[] { 1, 2, 3 });

        result.Success.Should().BeTrue();
        result.Text.Should().Be("Vision extracted snippet");
    }

    [Fact]
    public async Task RecognizeTextAsync_WhenServerReturns401_ReturnsAuthError()
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("{\"message\":\"Invalid API Key\"}")
            });

        var client = new MistralOcrClient(_defaultSettings, mockHandler.Object);
        var result = await client.RecognizeTextAsync(new byte[] { 1, 2, 3 });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Неверный API-ключ");
    }

    [Fact]
    public async Task RecognizeTextAsync_WhenServerReturns429_ReturnsRateLimitError()
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.TooManyRequests,
                Content = new StringContent("{\"message\":\"Rate limit exceeded\"}")
            });

        var client = new MistralOcrClient(_defaultSettings, mockHandler.Object);
        var result = await client.RecognizeTextAsync(new byte[] { 1, 2, 3 });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Превышен лимит");
    }
}
