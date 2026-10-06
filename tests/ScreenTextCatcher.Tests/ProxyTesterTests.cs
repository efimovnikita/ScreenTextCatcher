using System.Diagnostics;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using Moq;
using Moq.Protected;
using ScreenTextCatcher.Core.Models;
using ScreenTextCatcher.Core.Services;

namespace ScreenTextCatcher.Tests;

public class ProxyTesterTests
{
    [Fact]
    public void HttpHandlerFactory_WhenProxyDisabled_SetsUseProxyFalse()
    {
        var settings = new ProxySettings { Enabled = false };
        using var handler = HttpHandlerFactory.CreateHandler(settings);

        handler.UseProxy.Should().BeFalse();
    }

    [Fact]
    public void HttpHandlerFactory_WhenProxyEnabled_SetsWebProxyWithCorrectScheme()
    {
        var settings = new ProxySettings
        {
            Enabled = true,
            Type = ProxyType.Socks5,
            Host = "127.0.0.1",
            Port = 1080,
            Username = "user",
            Password = "password"
        };

        using var handler = HttpHandlerFactory.CreateHandler(settings);

        handler.UseProxy.Should().BeTrue();
        handler.Proxy.Should().NotBeNull();
        var webProxy = handler.Proxy as WebProxy;
        webProxy.Should().NotBeNull();
        webProxy!.Address.Should().Be(new Uri("socks5://127.0.0.1:1080"));
        webProxy.Credentials.Should().NotBeNull();
    }

    [Fact]
    public async Task TestMistralApiAsync_WhenApiKeyIsEmpty_ReturnsFailureImmediately()
    {
        var tester = new ProxyTester();
        var result = await tester.TestMistralApiAsync("", new ProxySettings());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("API ключ не указан");
    }

    [Fact]
    public async Task TestMistralApiAsync_WhenServerReturns401_ReturnsAuthFailure()
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
                Content = new StringContent("{\"message\":\"Unauthorized\"}")
            });

        var tester = new ProxyTester(mockHandler.Object);
        var result = await tester.TestMistralApiAsync("invalid_key", new ProxySettings());

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(401);
        result.Message.Should().Contain("Неверный API-ключ");
    }

    [Fact]
    public async Task TestMistralApiAsync_WhenServerReturns200_ReturnsSuccess()
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"data\":[]}")
            });

        var tester = new ProxyTester(mockHandler.Object);
        var result = await tester.TestMistralApiAsync("valid_key", new ProxySettings());

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Message.Should().Contain("успешно");
    }
}
