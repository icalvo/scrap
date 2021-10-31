using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
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
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static Task Literal(
            string adjacencyXPath,
            string adjacencyAttribute,
            string resourceXPath,
            string resourceAttribute,
            string destinationRootFolder,
            string destinationExpression,
            bool whatIf = true,
            string? rootUrl = null)
        {
            var appService = BuildScrapperApplicationService();

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
            var appService = BuildScrapperJobApplicationService();

            return appService.AddJobAsync(
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
                    rootUrl));
        }

        [Verb(IsDefault = true)]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static Task Scrap(
            string name,
            string? rootUrl = null,
            bool whatIf = true)
        {
            var appService = BuildScrapperApplicationService();

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

        private static ScrapperJobApplicationService BuildScrapperJobApplicationService()
        {
            var loggerFactory = BuildLoggerFactory();

            return
                new ScrapperJobApplicationService(
                    new LiteDbJobDefinitionRepository(
                        new LiteDatabase(GetExecutableDirectory()),
                        new Logger<LiteDbJobDefinitionRepository>(loggerFactory)),
                    new Logger<ScrapperJobApplicationService>(loggerFactory),
                    new ResourceRepositoryFactory(loggerFactory));
        }

        private static ScrapperApplicationService BuildScrapperApplicationService()
        {
            var loggerFactory = BuildLoggerFactory();

            return new ScrapperApplicationService(
                GraphSearch.DepthFirstSearch,
                new CachedPageRetriever(new Logger<CachedPageRetriever>(loggerFactory), new Logger<Page>(loggerFactory)),
                new LiteDbJobDefinitionRepository(new LiteDatabase(GetExecutableDirectory()), new Logger<LiteDbJobDefinitionRepository>(loggerFactory)),
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

        private static string? GetExecutableDirectory()
        {
            var executableDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location), "jobs.db");
            Console.WriteLine("DB dir: "+ executableDirectory);
            return executableDirectory;
        }
    }
}