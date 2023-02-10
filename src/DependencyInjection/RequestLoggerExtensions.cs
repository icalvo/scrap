using Microsoft.Extensions.Logging;

namespace Scrap.DependencyInjection;

public static class RequestLoggerExtensions
{
    public static void LogRequest(this ILogger logger, string method, string? url) =>
        logger.LogTrace("{Method} {Uri}", method, url);
}
