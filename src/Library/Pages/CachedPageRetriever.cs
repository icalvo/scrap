using System;
using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace Scrap.Pages
{
    public class CachedPageRetriever : IPageRetriever
    {
        private readonly ILogger<CachedPageRetriever> _logger;
        private readonly ILogger<Page> _pageLogger;
        private readonly HtmlWeb _web = new();
        private readonly Dictionary<Uri, Page> _pages = new();

        public CachedPageRetriever(ILogger<CachedPageRetriever> logger, ILogger<Page> pageLogger)
        {
            _logger = logger;
            _pageLogger = pageLogger;
        }

        public Page GetPage(Uri uri)
        {
            if (_pages.TryGetValue(uri, out var page))
            {
                _logger.LogInformation("CACHED {0}", uri);
                return page;
            }

            for (int i = 1; i <= 5; i++)
            {
                try
                {
                    Thread.Sleep(1000);
                    _logger.LogInformation("GET {Uri}", uri);
                    page = new Page(uri, _web.Load(uri.AbsoluteUri), this, _pageLogger);
                    _pages.Add(uri, page);
                    return page;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error: {Message}", ex.Message);
                }
            }
            
            throw new InvalidOperationException("Retried download too many times");
        }
    }
}