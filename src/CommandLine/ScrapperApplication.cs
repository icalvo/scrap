using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CLAP;
using CLAP.Interception;
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
    public class ScrapperApplication
    {
        private const int DefaultHttpRequestRetries = 5;
        private static readonly TimeSpan DefaultCacheTtl = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan DefaultHttpRequestDelayBetweenRetries = TimeSpan.FromSeconds(1);

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static Task Literal(
            string adjacencyXPath,
            string adjacencyAttribute,
            string resourceXPath,
            string resourceAttribute,
            string destinationRootFolder,
            string destinationExpression,
            int httpRequestRetries = DefaultHttpRequestRetries,
            TimeSpan? httpRequestDelayBetweenRetries = null,
            bool whatIf = true,
            string? rootUrl = null)
        {
            httpRequestDelayBetweenRetries ??= DefaultHttpRequestDelayBetweenRetries;
            var appService = BuildScrapperApplicationService(
                httpRequestRetries,
                (TimeSpan)httpRequestDelayBetweenRetries);

            return appService.ScrapAsync(
                new JobDefinition(
                    adjacencyXPath,
                    adjacencyAttribute,
                    resourceXPath,
                    resourceAttribute,
                    "filesystem",
                    new []{
                        destinationRootFolder,
                        destinationExpression,
                        whatIf.ToString()
                    },
                    rootUrl,
                    httpRequestRetries,
                    (TimeSpan)httpRequestDelayBetweenRetries),
                whatIf);
        }

        [Verb]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static Task Add(
            string name,
            string adjacencyXPath,
            string adjacencyAttribute,
            string resourceXPath,
            string resourceAttribute,
            string destinationRootFolder,
            string destinationExpression,
            int httpRequestRetries = DefaultHttpRequestRetries,
            TimeSpan? httpRequestDelayBetweenRetries = null,

            string? rootUrl = null)
        {
            var defsAppService = BuildJobDefinitionsApplicationService();
            
            return defsAppService.AddJobAsync(
                name,
                new JobDefinition(
                    adjacencyXPath,
                    adjacencyAttribute,
                    resourceXPath,
                    resourceAttribute,
                    "filesystem",
                    new []{
                        destinationRootFolder,
                        destinationExpression,
                        false.ToString()
                    },
                    rootUrl,
                    httpRequestRetries,
                    httpRequestDelayBetweenRetries ?? DefaultHttpRequestDelayBetweenRetries));
        }

        [Verb(IsDefault = true)]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static async Task Scrap(
            string name,
            string? rootUrl = null,
            bool whatIf = true)
        {
            var defsAppService = BuildJobDefinitionsApplicationService();
            var scrapJobDefinition = await defsAppService.GetJobAsync(name);
            scrapJobDefinition = new JobDefinition(scrapJobDefinition, rootUrl);
            
            if (scrapJobDefinition.RootUrl == null)
            {
                throw new Exception("No Root URL found as argument or in the job definition");
            }

            var scrapAppService = BuildScrapperApplicationService(scrapJobDefinition.HttpRequestRetries, scrapJobDefinition.HttpRequestDelayBetweenRetries);
            
            await scrapAppService.ScrapAsync(scrapJobDefinition, whatIf);            
        }

        [PostVerbExecution]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static void After(PostVerbExecutionContext context)
        {
            Console.WriteLine(context.Exception);
        }

        private static JobDefinitionsApplicationService BuildJobDefinitionsApplicationService()
        {
            var loggerFactory = BuildLoggerFactory();
            return
                new JobDefinitionsApplicationService(
                    new LiteDbJobDefinitionRepository(
                        new LiteDatabase(GetDbPath()),
                        new Logger<LiteDbJobDefinitionRepository>(loggerFactory)),
                    new Logger<JobDefinitionsApplicationService>(loggerFactory),
                    new ResourceRepositoryFactory(
                        new NullResourceDownloader(),
                        loggerFactory));
        }

        private static ScrapperApplicationService BuildScrapperApplicationService(
            int httpRequestRetries,
            TimeSpan httpRequestDelayBetweenRetries)
        {
            var loggerFactory = BuildLoggerFactory();
            var cacheLogger = loggerFactory.CreateLogger("Cache");
            var httpPolicy = Policy.CacheAsync(
                new MemoryCacheProvider(
                    new MemoryCache(new MemoryCacheOptions(), loggerFactory)),
                DefaultCacheTtl,
                (_, key) => { cacheLogger.LogInformation("CACHED {Uri}", key); },
                (_, _) => {  },
                (_, _) => {  },
                (_, _, _) => {  },
                (_, _, _) => {  })
                .WrapAsync(
                    Policy
                        .Handle<Exception>()
                        .WaitAndRetryAsync(
                            httpRequestRetries, _ => httpRequestDelayBetweenRetries,
                            (exception, span) =>
                            {
                                Console.WriteLine(exception.Message);
                                Thread.Sleep(span);
                            }));

            return new ScrapperApplicationService(
                GraphSearch.DepthFirstSearchAsync,
                new HttpPageRetriever(new Logger<HttpPageRetriever>(loggerFactory), loggerFactory, httpPolicy),
                new ResourceRepositoryFactory(
                    new HttpResourceDownloader(new Logger<HttpResourceDownloader>(loggerFactory), httpPolicy),
                    loggerFactory),
                new Logger<ScrapperApplicationService>(loggerFactory));
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

        private static string GetDbPath()
        {
            var directoryName = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? throw new Exception("Cannot find entry assembly path");
            var dbPath = Path.Combine(directoryName, "jobs.db");
            Console.WriteLine("DB dir: {0}", dbPath);
            return dbPath;
        }
    }
}