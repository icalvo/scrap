using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Polly;

namespace Scrap.Pages
{
    public class HttpPageRetriever : IPageRetriever
    {
        private readonly ILogger<HttpPageRetriever> _logger;
        private readonly IAsyncPolicy _executionPolicy;
        private readonly ILogger<Page> _pageLogger;
        private readonly HtmlWeb _web = new();

        public HttpPageRetriever(ILogger<HttpPageRetriever> logger, ILoggerFactory loggerFactory, IAsyncPolicy executionPolicy)
        {
            _logger = logger;
            _executionPolicy = executionPolicy;
            _pageLogger = new Logger<Page>(loggerFactory);
        }

        public Task<Page> GetPageAsync(Uri uri)
        {
            return _executionPolicy.ExecuteAsync(async _ =>
                {
                    _logger.LogInformation("GET {Uri}", uri);
                    return new Page(uri, await _web.LoadFromWebAsync(uri.AbsoluteUri), this, _pageLogger);
                },
                new Context(uri.AbsoluteUri));
        }
    }
}