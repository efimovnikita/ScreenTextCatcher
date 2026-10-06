namespace ScreenTextCatcher.Core.Models;

public record ProxyTestResult(
    bool Success,
    string Message,
    long ElapsedMilliseconds = 0,
    int? StatusCode = null
);
