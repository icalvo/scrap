using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Polly;
using Scrap.Domain.Downloads;
using Scrap.Domain.Pages;

namespace Scrap.Infrastructure;

public class HttpPageRetriever : IPageRetriever
{
    private readonly IDownloadStreamProvider _client;
    private readonly ILogger<HttpPageRetriever> _logger;
    private readonly ResiliencePipeline _noCachePipeline;
    private readonly ILogger<Page> _pageLogger;
    private readonly ResiliencePipeline _pipeline;

    public HttpPageRetriever(
        IDownloadStreamProvider client,
        ResiliencePipeline pipeline,
        ResiliencePipeline noCachePipeline,
        ILogger<HttpPageRetriever> logger,
        ILoggerFactory loggerFactory)
    {
        _client = client;
        _pipeline = pipeline;
        _noCachePipeline = noCachePipeline;
        _logger = logger;
        _pageLogger = new Logger<Page>(loggerFactory);
    }

    public Task<IPage> GetPageAsync(Uri uri) => GetPageAsync(uri, false);

    public Task<IPage> GetPageWithoutCacheAsync(Uri uri) => GetPageAsync(uri, true);

    private async Task<IPage> GetPageAsync(Uri uri, bool noCache)
    {
        var pipeline = noCache ? _noCachePipeline : _pipeline;
        var context = ResilienceContextPool.Shared.Get($"Page {uri.AbsoluteUri}", CancellationToken.None);
        try
        {
            return await pipeline.ExecuteAsync(
                async (ctx, state) =>
                {
                    await using var stream = await state.client.GetStreamAsync(state.uri);
                    HtmlDocument document = new();
                    document.Load(stream);
                    return (IPage)new Page(state.uri, document, state.retriever, state.pageLogger);
                },
                context,
                (client: _client, uri, retriever: this, pageLogger: _pageLogger));
        }
        finally
        {
            ResilienceContextPool.Shared.Return(context);
        }
    }
}
