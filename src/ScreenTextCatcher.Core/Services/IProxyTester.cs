using ScreenTextCatcher.Core.Models;

namespace ScreenTextCatcher.Core.Services;

public interface IProxyTester
{
    Task<ProxyTestResult> TestProxyConnectionAsync(ProxySettings proxy, CancellationToken cancellationToken = default);
    Task<ProxyTestResult> TestMistralApiAsync(string apiKey, ProxySettings proxy, CancellationToken cancellationToken = default);
}
