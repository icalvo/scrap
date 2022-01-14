using System;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Polly;
using Scrap.Downloads;

namespace Scrap.Pages;

public class HttpPageRetriever : IPageRetriever
{
    private readonly IDownloadStreamProvider _client;
    private readonly IAsyncPolicy _policy;
    private readonly ILogger<HttpPageRetriever> _logger;
    private readonly ILogger<Page> _pageLogger;

    public HttpPageRetriever(IDownloadStreamProvider client, IAsyncPolicy policy, ILogger<HttpPageRetriever> logger, ILoggerFactory loggerFactory)
    {
        _client = client;
        _policy = policy;
        _logger = logger;
        _pageLogger = new Logger<Page>(loggerFactory);
    }

    public Task<Page> GetPageAsync(Uri uri)
    {
        _logger.LogDebug("GET {Uri}", uri);
        return _policy.ExecuteAsync(async _ =>
        {
            var stream = await _client.GetStreamAsync(uri);
            HtmlDocument document = new();
            document.Load(stream);
            return new Page(uri, document, this, _pageLogger);
        }, new Context("Page " + uri.AbsoluteUri));
    }
}