using LiteDB;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly.Caching.Memory;
using Scrap.JobDefinitions;
using Scrap.Pages;
using Scrap.Resources;

namespace Scrap.DependencyInjection
{
    public static class DependencyInjection
    {
        public static readonly ILoggerFactory LoggerFactoryInstance = BuildLoggerFactory();

        public static JobDefinitionsApplicationService BuildJobDefinitionsApplicationService(IConfiguration config)
        {
            return
                new JobDefinitionsApplicationService(
                    new LiteDbJobDefinitionRepository(
                        new LiteDatabase(new ConnectionString(config["Database"])),
                        new Logger<LiteDbJobDefinitionRepository>(LoggerFactoryInstance)),
                    new ResourceRepositoryConfigurationValidator(LoggerFactoryInstance));
        }

        public static ScrapperApplicationService BuildScrapperApplicationService(
            IConfiguration config)
        {
            var liteDatabase = new LiteDatabase(new ConnectionString(config["Database"]));
            return new ScrapperApplicationService(
                GraphSearch.DepthFirstSearchAsync,
                new PageRetrieverFactory(LoggerFactoryInstance, BuildMemoryCacheProvider()),
                new ResourceRepositoryFactory(
                    new ResourceDownloaderFactory(LoggerFactoryInstance),
                    LoggerFactoryInstance),
                new Logger<ScrapperApplicationService>(LoggerFactoryInstance),
                new PageMarkerRepositoryFactory(liteDatabase, LoggerFactoryInstance),
                new HttpPolicyFactory(LoggerFactoryInstance, BuildMemoryCacheProvider()),
                new LiteDbJobDefinitionRepository(
                    liteDatabase,
                    new Logger<LiteDbJobDefinitionRepository>(LoggerFactoryInstance)));
        }

        private static MemoryCacheProvider BuildMemoryCacheProvider()
        {
            var cacheProvider = new MemoryCacheProvider(
                new MemoryCache(new MemoryCacheOptions(), LoggerFactoryInstance));
            return cacheProvider;
        }

        private static ILoggerFactory BuildLoggerFactory()
        {
            return LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddSimpleConsole(options => options.SingleLine = true);
            });
        }
    }
}