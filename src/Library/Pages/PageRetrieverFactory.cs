using System;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Caching;

namespace Scrap.Pages
{
    public class PageRetrieverFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IAsyncCacheProvider _cacheProvider;

        public PageRetrieverFactory(ILoggerFactory loggerFactory, IAsyncCacheProvider cacheProvider)
        {
            _loggerFactory = loggerFactory;
            _cacheProvider = cacheProvider;
        }

        public IPageRetriever Build(
            IAsyncPolicy httpPolicy)
        {
            return new HttpPageRetriever(
                new Logger<HttpPageRetriever>(_loggerFactory), _loggerFactory, httpPolicy);
        }
    }
}