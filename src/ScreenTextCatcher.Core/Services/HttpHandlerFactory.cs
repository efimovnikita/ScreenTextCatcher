using System.Net;
using System.Net.Http;
using ScreenTextCatcher.Core.Models;

namespace ScreenTextCatcher.Core.Services;

public static class HttpHandlerFactory
{
    public static SocketsHttpHandler CreateHandler(ProxySettings? proxy, TimeSpan? connectTimeout = null)
    {
        var handler = new SocketsHttpHandler
        {
            ConnectTimeout = connectTimeout ?? TimeSpan.FromSeconds(10),
            PooledConnectionLifetime = TimeSpan.FromMinutes(5)
        };

        if (proxy != null && proxy.Enabled && !string.IsNullOrWhiteSpace(proxy.Host))
        {
            var scheme = proxy.Type switch
            {
                ProxyType.Socks5 => "socks5",
                ProxyType.Https => "https",
                _ => "http"
            };

            var proxyUri = new Uri($"{scheme}://{proxy.Host}:{proxy.Port}");
            var webProxy = new WebProxy(proxyUri);

            if (!string.IsNullOrWhiteSpace(proxy.Username))
            {
                webProxy.Credentials = new NetworkCredential(proxy.Username, proxy.Password ?? string.Empty);
            }

            handler.Proxy = webProxy;
            handler.UseProxy = true;
        }
        else
        {
            handler.UseProxy = false;
        }

        return handler;
    }
}
