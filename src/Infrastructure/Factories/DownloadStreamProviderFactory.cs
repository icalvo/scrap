using Microsoft.Extensions.Logging;
using Polly;
using Scrap.Domain.Downloads;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;

namespace Scrap.Infrastructure.Factories;

public class DownloadStreamProviderFactory : IDownloadStreamProviderFactory
{
    private readonly IAsyncPolicyFactory _asyncPolicyFactory;
    private readonly ILoggerFactory _loggerFactory;

    public DownloadStreamProviderFactory(
        IAsyncPolicyFactory asyncPolicyFactory,
        ILoggerFactory loggerFactory)
    {
        _asyncPolicyFactory = asyncPolicyFactory;
        _loggerFactory = loggerFactory;
    }

    public IDownloadStreamProvider Build(Job job)
    {
        const string protocol = "http";
        var logger = _loggerFactory.CreateLogger<HttpClientDownloadStreamProvider>();
        var pipeline = _asyncPolicyFactory.Build(job, AsyncPolicyConfiguration.WithoutCache);

        switch (protocol)
        {
            case "http":
            case "https":
                DelegatingHandler[] wrappingHandlers = { new PollyMessageHandler(pipeline), new LoggingHandler(logger) };
                HttpMessageHandler primaryHandler = new HttpClientHandler();

                var handler = Enumerable.Reverse(wrappingHandlers).Aggregate(
                    primaryHandler,
                    (HttpMessageHandler accum, DelegatingHandler item) =>
                    {
                        item.InnerHandler = accum;
                        return item;
                    });

                var httpClient = new HttpClient(handler);
                return new HttpClientDownloadStreamProvider(httpClient);
            default:
                throw new ArgumentException($"Unknown URI protocol {protocol}", nameof(protocol));
        }
    }

    private class PollyMessageHandler : DelegatingHandler
    {
        private readonly ResiliencePipeline _pipeline;

        public PollyMessageHandler(ResiliencePipeline pipeline) => _pipeline = pipeline;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var context = ResilienceContextPool.Shared.Get(
                request.RequestUri?.AbsoluteUri ?? string.Empty,
                cancellationToken);
            try
            {
                return await _pipeline.ExecuteAsync(
                    async (ctx, state) => await base.SendAsync(state, ctx.CancellationToken),
                    context,
                    request);
            }
            finally
            {
                ResilienceContextPool.Shared.Return(context);
            }
        }
    }

    private class LoggingHandler : DelegatingHandler
    {
        private readonly ILogger _logger;

        public LoggingHandler(ILogger logger) => _logger = logger;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            _logger.LogRequest(request.Method.ToString(), request.RequestUri?.AbsoluteUri);
            return base.SendAsync(request, cancellationToken);
        }
    }
}
