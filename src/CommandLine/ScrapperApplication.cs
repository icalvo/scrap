using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CLAP;
using CLAP.Interception;
using CLAP.Validation;
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
        private static readonly ILoggerFactory LoggerFactory = BuildLoggerFactory();
        
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static Task Literal(
            string adjacencyXPath,
            string resourceXPath,
            string resourceAttribute,
            [DirectoryExists]string destinationRootFolder,
            string destinationExpression,
            string? adjacencyAttribute = null,
            int? httpRequestRetries = null,
            TimeSpan? httpRequestDelayBetweenRetries = null,
            bool whatIf = true,
            string? rootUrl = null)
        {
            var appService = BuildScrapperApplicationService(
                httpRequestRetries ?? DefaultHttpRequestRetries,
                httpRequestDelayBetweenRetries ?? DefaultHttpRequestDelayBetweenRetries);

            return appService.ScrapAsync(
                new JobDefinition(
                    adjacencyXPath,
                    adjacencyAttribute ?? "href",
                    resourceXPath,
                    resourceAttribute,
                    "filesystem",
                    new []{
                        destinationRootFolder,
                        destinationExpression,
                        whatIf.ToString()
                    },
                    rootUrl,
                    httpRequestRetries ?? DefaultHttpRequestRetries,
                    httpRequestDelayBetweenRetries ?? DefaultHttpRequestDelayBetweenRetries),
                whatIf);
        }

        [Verb]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static Task Add(
            string name,
            string adjacencyXPath,
            string resourceXPath,
            string resourceAttribute,
            [DirectoryExists]string destinationRootFolder,
            string destinationExpression,
            string? adjacencyAttribute = null,
            int? httpRequestRetries = null,
            TimeSpan? httpRequestDelayBetweenRetries = null,
            string? rootUrl = null)
        {
            var defsAppService = BuildJobDefinitionsApplicationService();
            
            var jobDefinition = new JobDefinition(
                adjacencyXPath,
                adjacencyAttribute ?? "href",
                resourceXPath,
                resourceAttribute,
                "filesystem",
                new []{
                    destinationRootFolder,
                    destinationExpression,
                    false.ToString()
                },
                rootUrl,
                httpRequestRetries ?? DefaultHttpRequestRetries,
                httpRequestDelayBetweenRetries ?? DefaultHttpRequestDelayBetweenRetries);

            jobDefinition.Log(LoggerFactory.CreateLogger("Console"));
            
            return defsAppService.AddJobAsync(
                name,
                jobDefinition);
        }

        [Verb(Aliases = "del")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static Task Delete(string name)
        {
            var defsAppService = BuildJobDefinitionsApplicationService();
            
            return defsAppService.DeleteJobAsync(name);
        }

        [Verb]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static async Task Show(string name)
        {
            var defsAppService = BuildJobDefinitionsApplicationService();
            
            var jobDefinition = await defsAppService.GetJobAsync(name);
            jobDefinition.Log(LoggerFactory.CreateLogger("Console"));
        }

        [Verb(IsDefault = true)]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static async Task Scrap(
            string? name = null,
            string? rootUrl = null,
            bool whatIf = true)
        {
            var defsAppService = BuildJobDefinitionsApplicationService();
            JobDefinition scrapJobDefinition;
            if (name == null)
            {
                if (rootUrl == null)
                {
                    throw new Exception("No Root URL found as argument or in the job definition");
                }

                scrapJobDefinition = await defsAppService.FindJobByRootUrlAsync(rootUrl);
            }
            else
            {
                scrapJobDefinition = await defsAppService.GetJobAsync(name);

                scrapJobDefinition = new JobDefinition(scrapJobDefinition, rootUrl);
            
                if (scrapJobDefinition.RootUrl == null)
                {
                    throw new Exception("No Root URL found as argument or in the job definition");
                }
            }


            var scrapAppService = BuildScrapperApplicationService(
                scrapJobDefinition.HttpRequestRetries,
                scrapJobDefinition.HttpRequestDelayBetweenRetries);
            
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
            return
                new JobDefinitionsApplicationService(
                    new LiteDbJobDefinitionRepository(
                        new LiteDatabase(GetDbPath()),
                        new Logger<LiteDbJobDefinitionRepository>(LoggerFactory)),
                    new Logger<JobDefinitionsApplicationService>(LoggerFactory),
                    new ResourceRepositoryFactory(
                        new NullResourceDownloader(),
                        LoggerFactory));
        }

        private static ScrapperApplicationService BuildScrapperApplicationService(
            int httpRequestRetries,
            TimeSpan httpRequestDelayBetweenRetries)
        {
            var cacheLogger = LoggerFactory.CreateLogger("Cache");
            var httpPolicy = Policy.CacheAsync(
                new MemoryCacheProvider(
                    new MemoryCache(new MemoryCacheOptions(), LoggerFactory)),
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
                new HttpPageRetriever(new Logger<HttpPageRetriever>(LoggerFactory), LoggerFactory, httpPolicy),
                new ResourceRepositoryFactory(
                    new HttpResourceDownloader(new Logger<HttpResourceDownloader>(LoggerFactory), httpPolicy),
                    LoggerFactory),
                new Logger<ScrapperApplicationService>(LoggerFactory));
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
}