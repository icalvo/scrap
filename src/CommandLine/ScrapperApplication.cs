using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CLAP;
using CLAP.Interception;
using LiteDB;
using Microsoft.Extensions.Logging;
using Scrap.JobDefinitions;
using Scrap.Pages;
using Scrap.Resources;

namespace Scrap.CommandLine
{
    public class ScrapperApplication
    {
        [Verb(IsDefault = true)]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static Task Scrap(
            string adjacencyXPath,
            string adjacencyAttribute,
            string resourceXPath,
            string resourceAttribute,
            string destinationRootFolder,
            string destinationExpression,
            bool whatIf = true,
            string? rootUrl = null)
        {
            var appService = BuildApplicationService();


            return appService.ScrapAsync(
                new ScrapJobDefinition(
                    adjacencyXPath,
                    adjacencyAttribute,
                    resourceXPath,
                    resourceAttribute,
                    destinationRootFolder,
                    destinationExpression,
                    rootUrl),
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
            string? rootUrl = null)
        {
            var loggerFactory = BuildLoggerFactory();

            var appService = new ScrapperJobApplicationService(
                new LiteDbJobDefinitionRepository(
                    new LiteDatabase("jobs.db"),
                    new Logger<LiteDbJobDefinitionRepository>(loggerFactory)),
                new Logger<ScrapperJobApplicationService>(loggerFactory));

            return appService.AddJob(
                name,
                new ScrapJobDefinition(
                    adjacencyXPath,
                    adjacencyAttribute,
                    resourceXPath,
                    resourceAttribute,
                    destinationRootFolder,
                    destinationExpression,
                    rootUrl));
        }

        [Verb]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static Task Db(
            string name,
            string? rootUrl = null,
            bool whatIf = true)
        {
            var appService = BuildApplicationService();

            return appService.ScrapAsync(
                name,
                whatIf,
                rootUrl);
        }

        [PostVerbExecution]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static void After(PostVerbExecutionContext context)
        {
            Console.WriteLine(context.Exception);
        }

        private static ScrapperApplicationService BuildApplicationService()
        {
            var loggerFactory = BuildLoggerFactory();

            return new ScrapperApplicationService(
                GraphSearch.DepthFirstSearch,
                new CachedPageRetriever(new Logger<CachedPageRetriever>(loggerFactory), new Logger<Page>(loggerFactory)),
                new LiteDbJobDefinitionRepository(new LiteDatabase("jobs.db"), new Logger<LiteDbJobDefinitionRepository>(loggerFactory)),
                new ResourceRepositoryFactory(loggerFactory),
                new Logger<ScrapperApplicationService>(loggerFactory));
        }

        private static ILoggerFactory BuildLoggerFactory()
        {
            return LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddSimpleConsole(options => options.SingleLine = true);
            });
        }
    }
}