using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Polly;
using Scrap.Domain.Downloads;

namespace Scrap.Domain.Pages;

public class HttpPageRetriever : IPageRetriever
{
    private readonly IDownloadStreamProvider _client;
    private readonly ILogger<HttpPageRetriever> _logger;
    private readonly IAsyncPolicy _noCachePolicy;
    private readonly ILogger<Page> _pageLogger;
    private readonly IAsyncPolicy _policy;

    public HttpPageRetriever(
        IDownloadStreamProvider client,
        IAsyncPolicy policy,
        IAsyncPolicy noCachePolicy,
        ILogger<HttpPageRetriever> logger,
        ILoggerFactory loggerFactory)
    {
        _client = client;
        _policy = policy;
        _noCachePolicy = noCachePolicy;
        _logger = logger;
        _pageLogger = new Logger<Page>(loggerFactory);
    }

    public Task<IPage> GetPageAsync(Uri uri) => GetPageAsync(uri, false);

    public Task<IPage> GetPageWithoutCacheAsync(Uri uri) => GetPageAsync(uri, true);

    private Task<IPage> GetPageAsync(Uri uri, bool noCache)
    {
        _logger.LogTrace("GET {Uri}", uri);
        var policy = noCache ? _noCachePolicy : _policy;
        return policy.ExecuteAsync<IPage>(
            async _ =>
            {
                var stream = await _client.GetStreamAsync(uri);
                HtmlDocument document = new();
                document.Load(stream);
                return new Page(uri, document, this, _pageLogger);
            },
            new Context($"Page {uri.AbsoluteUri}"));
    }
}
