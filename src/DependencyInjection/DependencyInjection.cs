using LiteDB;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly.Caching.Memory;
using Scrap.Downloads;
using Scrap.Graphs;
using Scrap.JobDefinitions.LiteDb;
using Scrap.Jobs.LiteDb;
using Scrap.Pages;
using Scrap.ResourceDownloaders;
using Scrap.Resources;

namespace Scrap.DependencyInjection
{
    public static class DependencyInjection
    {
        public static JobDefinitionsApplicationService BuildJobDefinitionsApplicationService(
            IConfiguration config,
            ILoggerFactory loggerFactory)
        {
            return
                new JobDefinitionsApplicationService(
                    new LiteDbJobDefinitionRepository(
                        new LiteDatabase(new ConnectionString(config["Database"])),
                        new Logger<LiteDbJobDefinitionRepository>(loggerFactory)),
                    new Logger<JobDefinitionsApplicationService>(loggerFactory),
                    loggerFactory);
        }

        public static JobApplicationService BuildScrapperApplicationService(
            IConfiguration config,
            ILoggerFactory loggerFactory)
        {
            var liteDatabase = new LiteDatabase(new ConnectionString(config["Database"]));
            return new JobApplicationService(
                new DepthFirstGraphSearch(),
                new Logger<JobApplicationService>(loggerFactory),
                new PageMarkerRepositoryFactory(liteDatabase, loggerFactory),
                new HttpPolicyFactory(loggerFactory, BuildMemoryCacheProvider(loggerFactory)),
                new DownloadStreamProviderFactory(),
                new ResourceProcessorFactory(loggerFactory),
                loggerFactory);
        }

        private static MemoryCacheProvider BuildMemoryCacheProvider(ILoggerFactory loggerFactory)
        {
            var cacheProvider = new MemoryCacheProvider(
                new MemoryCache(new MemoryCacheOptions(), loggerFactory));
            return cacheProvider;
        }
    }
}