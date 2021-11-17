using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Scrap.Downloads;

namespace Scrap.Pages
{
    public class HttpPageRetriever : IPageRetriever
    {
        private readonly IDownloadStreamProvider _client;
        private readonly ILogger<HttpPageRetriever> _logger;
        private readonly ILogger<Page> _pageLogger;

        public HttpPageRetriever(IDownloadStreamProvider client, ILogger<HttpPageRetriever> logger, ILoggerFactory loggerFactory)
        {
            _client = client;
            _logger = logger;
            _pageLogger = new Logger<Page>(loggerFactory);
        }

        public async Task<Page> GetPageAsync(Uri uri)
        {
            _logger.LogDebug("GET {Uri}", uri);
            var stream = await _client.GetStreamAsync(uri);
            HtmlDocument document = new();
            document.Load(stream);
            return new Page(uri, document, this, _pageLogger);
        }
    }
}