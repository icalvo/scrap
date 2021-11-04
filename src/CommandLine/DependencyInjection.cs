using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Caching.Memory;
using Scrap.JobDefinitions;
using Scrap.Pages;
using Scrap.Resources;

namespace Scrap.CommandLine
{
    public static class DependencyInjection
    {
        private static readonly TimeSpan DefaultCacheTtl = TimeSpan.FromMinutes(5);
        private static readonly LiteDatabase Database = new(GetDbPath());
        
        public static readonly ILoggerFactory LoggerFactoryInstance = BuildLoggerFactory();

        public static JobDefinitionsApplicationService BuildJobDefinitionsApplicationService()
        {
            return
                new JobDefinitionsApplicationService(
                    new LiteDbJobDefinitionRepository(
                        Database,
                        new Logger<LiteDbJobDefinitionRepository>(LoggerFactoryInstance)),
                    new Logger<JobDefinitionsApplicationService>(LoggerFactoryInstance),
                    new ResourceRepositoryFactory(
                        new NullResourceDownloader(),
                        LoggerFactoryInstance));
        }

        public static ScrapperApplicationService BuildScrapperApplicationService(
            int httpRequestRetries,
            TimeSpan httpDelay,
            bool fullScan)
        {
            var cacheLogger = LoggerFactoryInstance.CreateLogger("Cache");
            var cachePolicy = Policy.CacheAsync(
                new MemoryCacheProvider(
                    new MemoryCache(new MemoryCacheOptions(), LoggerFactoryInstance)),
                DefaultCacheTtl,
                (_, key) => { cacheLogger.LogInformation("CACHED {Uri}", key); },
                (_, _) => {  },
                (_, _) => {  },
                (_, _, _) => {  },
                (_, _, _) => {  });
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    httpRequestRetries,
                    _ => TimeSpan.Zero,
                    (exception, _) =>
                    {
                        Console.WriteLine(exception.Message);
                    });

            var httpPolicy = Policy.WrapAsync(
                cachePolicy,
                retryPolicy,
                new AsyncDelayPolicy(httpDelay));

            return new ScrapperApplicationService(
                GraphSearch.DepthFirstSearchAsync,
                new HttpPageRetriever(new Logger<HttpPageRetriever>(LoggerFactoryInstance), LoggerFactoryInstance, httpPolicy),
                new ResourceRepositoryFactory(
                    new HttpResourceDownloader(new Logger<HttpResourceDownloader>(LoggerFactoryInstance), httpPolicy),
                    LoggerFactoryInstance),
                new Logger<ScrapperApplicationService>(LoggerFactoryInstance),
                new LiteDbPageMarkerRepository(
                    Database,
                    new Logger<LiteDbPageMarkerRepository>(LoggerFactoryInstance),
                    disableExists: fullScan));
        }

        private static ILoggerFactory BuildLoggerFactory()
        {
            return Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddSimpleConsole(options => options.SingleLine = true);
            });
        }

        private static string GetDbPath()
        {
            var directoryName = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? throw new Exception("Cannot find entry assembly path");
            var dbPath = Path.Combine(directoryName, "jobs.db");
            Console.WriteLine("DB dir: {0}", dbPath);
            return dbPath;
        }
    }

    public class AsyncDelayPolicy : AsyncPolicy
    {
        private readonly TimeSpan _delay;

        public AsyncDelayPolicy(TimeSpan delay)
        {
            _delay = delay;
        }

        public static AsyncDelayPolicy Create(TimeSpan delay)
        {
            return new AsyncDelayPolicy(delay);
        }

        protected override async Task<TResult> ImplementationAsync<TResult>(
            Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            await Task.Delay(_delay, cancellationToken);
            return await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }
    }
    
}