using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CLAP;
using CLAP.Interception;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scrap.JobDefinitions;
using Scrap.Jobs;
using Scrap.Resources;
using Scrap.Resources.FileSystem;
using static Scrap.DependencyInjection.DependencyInjection;

namespace Scrap.CommandLine
{
    public class ScrapCommandLine
    {
        private static readonly IConfiguration Configuration;
        private static readonly ILoggerFactory LoggerFactoryInstance = BuildLoggerFactory();

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

        static ScrapCommandLine()
        {
            var directoryName = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? throw new Exception("Cannot find entry assembly path");
            var dbPath = Path.Combine(directoryName, "jobs.db");
            Console.WriteLine("DB dir: {0}", dbPath);
            Configuration =
                new ConfigurationBuilder()
                    .AddInMemoryCollection(new[] { new KeyValuePair<string, string>("Database", $"Filename={dbPath};Connection=shared") })
                    .Build();
        }
        [Verb(Description = "Executes a job definition provided with the arguments")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static async Task Literal(
            [Required]
            [Description("X-Path that locates tags (usually <a>) that contains links to be navigated by Scrap.")]
            string adjacencyXPath,
            [Required]
            [Description("X-Path that locates tags (usually <a>) that contains links to be navigated by Scrap.")]
            string resourceXPath,
            [Required]
            string resourceAttribute,
            [Required]
            string destinationRootFolder,
            [Required]string[] destinationExpression,
            [Required]
            string rootUrl,
            string? adjacencyAttribute = null,
            int? httpRequestRetries = null,
            TimeSpan? httpRequestDelayBetweenRetries = null,
            bool whatIf = false,
            bool fullScan = false)
        {
            var appService = BuildScrapperApplicationService(Configuration, LoggerFactoryInstance);

            await appService.RunAsync(
                new NewJobDto(
                    adjacencyXPath,
                    adjacencyAttribute ?? "href",
                    resourceXPath,
                    resourceAttribute,
                    new FileSystemResourceProcessorConfiguration(
                        destinationExpression,
                        destinationRootFolder),
                    rootUrl,
                    httpRequestRetries,
                    httpRequestDelayBetweenRetries,
                    whatIf,
                    fullScan));
        }

        [Verb(Description = "Adds to the database a job definition provided with the arguments")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static Task Add(
            [Required]string name,
            [Required]string adjacencyXPath,
            [Required]string resourceXPath,
            [Required]string resourceAttribute,
            [Required]string destinationRootFolder,
            [Required]string[] destinationExpression,
            string? adjacencyAttribute = null,
            int? httpRequestRetries = null,
            TimeSpan? httpRequestDelayBetweenRetries = null,
            string? rootUrl = null,
            string? urlPattern = null)
        {
            var defsAppService = BuildJobDefinitionsApplicationService(Configuration, LoggerFactoryInstance);
            
            var jobDefinition = new NewJobDefinitionDto(
                name,
                adjacencyXPath,
                adjacencyAttribute ?? "href",
                resourceXPath,
                resourceAttribute,
                new FileSystemResourceProcessorConfiguration(
                    destinationExpression,
                    destinationRootFolder),
                rootUrl,
                httpRequestRetries,
                httpRequestDelayBetweenRetries,
                urlPattern);

            jobDefinition.Log(LoggerFactoryInstance.CreateLogger("Console"));
            
            return defsAppService.AddJobAsync(jobDefinition);
        }

        [Verb(Aliases = "del", Description = "Deletes a job definition from the database")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static async Task Delete(string name)
        {
            var defsAppService = BuildJobDefinitionsApplicationService(Configuration, LoggerFactoryInstance);
            var jobDef = await defsAppService.FindJobByNameAsync(name);
            if (jobDef != null)
            {
                await defsAppService.DeleteJobAsync(jobDef.Id);
            }
        }

        [Verb(Description = "Shows a job definition from the database")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static async Task Show(string name)
        {
            var defsAppService = BuildJobDefinitionsApplicationService(Configuration, LoggerFactoryInstance);
            
            var jobDefinition = await defsAppService.FindJobByNameAsync(name);
            var logger = LoggerFactoryInstance.CreateLogger("Console");
            if (jobDefinition == null)
            {
                logger.LogInformation("Not found!");                
            }
            else
            {
                jobDefinition.Log(logger);
            }
        }

        [Verb(IsDefault = true, Description = "Executes a job definition from the database")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static async Task Scrap(
            string? name = null,
            string? rootUrl = null,
            bool whatIf = false,
            bool fullScan = false)
        {
            var definitionsApplicationService = BuildJobDefinitionsApplicationService(Configuration, LoggerFactoryInstance);
            var scrapAppService = BuildScrapperApplicationService(Configuration, LoggerFactoryInstance);
            JobDefinitionDto? jobDef = null;
            if (name != null)
            {
                jobDef = await definitionsApplicationService.FindJobByNameAsync(name);
            }
            else if (rootUrl != null)
            {
                jobDef = await definitionsApplicationService.FindJobByRootUrlAsync(rootUrl);
            }

            if (jobDef == null)
            {
                return;
            }

            var newJob = new NewJobDto(jobDef, rootUrl, whatIf, fullScan, null);
            await scrapAppService.RunAsync(newJob);
        }

        [Verb(Description = "Executes a job definition from the database, but only lists resources (no download)")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static async Task List(
            string? name = null,
            string? rootUrl = null,
            bool whatIf = false,
            bool fullScan = false)
        {
            var loggerFactory = LoggerFactory.Create(_ => { });
            var definitionsApplicationService = BuildJobDefinitionsApplicationService(Configuration, loggerFactory);
            var scrapAppService = BuildScrapperApplicationService(Configuration, loggerFactory);
            JobDefinitionDto? jobDef = null;
            if (name != null)
            {
                jobDef = await definitionsApplicationService.FindJobByNameAsync(name);
            }
            else if (rootUrl != null)
            {
                jobDef = await definitionsApplicationService.FindJobByRootUrlAsync(rootUrl);
            }

            if (jobDef == null)
            {
                return;
            }

            var newJob = new NewJobDto(jobDef, rootUrl, whatIf, fullScan, new ListResourceProcessorConfiguration(""));
            await scrapAppService.RunAsync(newJob);
        }

        [PostVerbExecution]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static void After(PostVerbExecutionContext context)
        {
            Console.WriteLine(context.Exception);
        }
    }
}