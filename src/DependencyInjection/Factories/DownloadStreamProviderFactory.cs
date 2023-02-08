using Microsoft.Extensions.Logging;
using Polly;
using Scrap.Domain;
using Scrap.Domain.Downloads;
using Scrap.Domain.Jobs;

namespace Scrap.DependencyInjection.Factories;

public class DownloadStreamProviderFactory : IFactory<Job, IDownloadStreamProvider>
{
    private readonly IFactory<Job, IAsyncPolicy> _asyncPolicyFactory;
    private readonly ILoggerFactory _loggerFactory;

    public DownloadStreamProviderFactory(
        IFactory<Job, IAsyncPolicy> asyncPolicyFactory,
        ILoggerFactory loggerFactory)
    {
        _asyncPolicyFactory = asyncPolicyFactory;
        _loggerFactory = loggerFactory;
    }

    public IDownloadStreamProvider Build(Job job)
    {
        const string protocol = "http";
        var logger = _loggerFactory.CreateLogger<HttpClientDownloadStreamProvider>();
        var policy = _asyncPolicyFactory.Build(job);
        DelegatingHandler[] wrappingHandlers = { new PollyMessageHandler(policy), new LoggingHandler(logger) };
        HttpMessageHandler primaryHandler = new HttpClientHandler();

        var handler = wrappingHandlers.Reverse().Aggregate(primaryHandler, (accum, item) =>
        {
            item.InnerHandler = accum;
            return item;
        });

        switch (protocol)
        {
            case "http":
            case "https":
                var httpClient = new HttpClient(handler);
                return new HttpClientDownloadStreamProvider(httpClient);
            default:
                throw new ArgumentException($"Unknown URI protocol {protocol}", nameof(protocol));
        }
    }


    private class PollyMessageHandler : DelegatingHandler
    {
        private readonly IAsyncPolicy _policy;

        public PollyMessageHandler(IAsyncPolicy policy)
        {
            _policy = policy;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            _policy.ExecuteAsync(
                (_, ct) => base.SendAsync(request, ct),
                new Context(request.RequestUri?.AbsoluteUri), cancellationToken);
    }

    private class LoggingHandler : DelegatingHandler
    {
        private readonly ILogger _logger;

        public LoggingHandler(ILogger logger)
        {
            _logger = logger;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            _logger.LogRequest(request.Method.ToString(), request.RequestUri?.AbsoluteUri);
            return base.SendAsync(request, cancellationToken);
        }
    }
}
