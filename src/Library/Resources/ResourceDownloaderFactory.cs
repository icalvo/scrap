using Microsoft.Extensions.Logging;
using Polly;

namespace Scrap.Resources
{
    public class ResourceDownloaderFactory
    {
        private readonly ILoggerFactory _loggerFactoryInstance;
        private readonly bool _enabled;

        public ResourceDownloaderFactory(ILoggerFactory loggerFactoryInstance, bool enabled = true)
        {
            _loggerFactoryInstance = loggerFactoryInstance;
            _enabled = enabled;
        }

        public IResourceDownloader Build(IAsyncPolicy httpPolicy)
        {
            if (_enabled)
            {
                return new HttpResourceDownloader(new Logger<HttpResourceDownloader>(_loggerFactoryInstance), httpPolicy);
            }

            return new NullResourceDownloader();
        }
    }
}