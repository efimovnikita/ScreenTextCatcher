using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using ScreenTextCatcher.Core.Models;

namespace ScreenTextCatcher.Core.Services;

public class ProxyTester : IProxyTester
{
    private readonly HttpMessageHandler? _testHandler;

    public ProxyTester(HttpMessageHandler? testHandler = null)
    {
        _testHandler = testHandler;
    }

    public async Task<ProxyTestResult> TestProxyConnectionAsync(ProxySettings proxy, CancellationToken cancellationToken = default)
    {
        if (proxy == null || !proxy.Enabled)
        {
            return new ProxyTestResult(true, "Прокси отключен в настройках (используется прямое подключение)");
        }

        if (string.IsNullOrWhiteSpace(proxy.Host))
        {
            return new ProxyTestResult(false, "Не указан адрес хоста прокси-сервера");
        }

        var sw = Stopwatch.StartNew();
        try
        {
            using var handler = _testHandler ?? HttpHandlerFactory.CreateHandler(proxy, TimeSpan.FromSeconds(8));
            using var client = new HttpClient(handler, disposeHandler: _testHandler == null)
            {
                Timeout = TimeSpan.FromSeconds(10)
            };

            using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.mistral.ai");
            using var response = await client.SendAsync(request, cancellationToken);
            sw.Stop();

            return new ProxyTestResult(
                true,
                $"Прокси-сервер доступен! Время отклика: {sw.ElapsedMilliseconds} мс",
                sw.ElapsedMilliseconds,
                (int)response.StatusCode);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new ProxyTestResult(false, "Проверка отменена пользователем");
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new ProxyTestResult(false, $"Ошибка подключения к прокси: {ex.Message}", sw.ElapsedMilliseconds);
        }
    }

    public async Task<ProxyTestResult> TestMistralApiAsync(string apiKey, ProxySettings proxy, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return new ProxyTestResult(false, "Mistral API ключ не указан");
        }

        var sw = Stopwatch.StartNew();
        try
        {
            using var handler = _testHandler ?? HttpHandlerFactory.CreateHandler(proxy, TimeSpan.FromSeconds(8));
            using var client = new HttpClient(handler, disposeHandler: _testHandler == null)
            {
                Timeout = TimeSpan.FromSeconds(12)
            };

            using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.mistral.ai/v1/models");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey.Trim());

            using var response = await client.SendAsync(request, cancellationToken);
            sw.Stop();

            if (response.IsSuccessStatusCode)
            {
                return new ProxyTestResult(
                    true,
                    $"API-ключ действителен, подключение успешно! Время отклика: {sw.ElapsedMilliseconds} мс",
                    sw.ElapsedMilliseconds,
                    (int)response.StatusCode);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return new ProxyTestResult(
                    false,
                    "Неверный API-ключ Mistral (HTTP 401 Unauthorized)",
                    sw.ElapsedMilliseconds,
                    401);
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return new ProxyTestResult(
                    false,
                    "Доступ запрещен для данного API-ключа (HTTP 403 Forbidden)",
                    sw.ElapsedMilliseconds,
                    403);
            }

            if (response.StatusCode == HttpStatusCode.ProxyAuthenticationRequired)
            {
                return new ProxyTestResult(
                    false,
                    "Прокси требует авторизации (HTTP 407 Proxy Authentication Required)",
                    sw.ElapsedMilliseconds,
                    407);
            }

            return new ProxyTestResult(
                false,
                $"Сервер вернул статус {(int)response.StatusCode} {response.ReasonPhrase}",
                sw.ElapsedMilliseconds,
                (int)response.StatusCode);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new ProxyTestResult(false, "Проверка отменена пользователем");
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new ProxyTestResult(false, $"Ошибка сетевого подключения: {ex.Message}", sw.ElapsedMilliseconds);
        }
    }
}
