using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CLAP;
using CLAP.Interception;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Scrap.JobDefinitions;
using Scrap.Resources.FileSystem;
using static Scrap.DependencyInjection.DependencyInjection;

namespace Scrap.CommandLine
{
    public class ScrapCommandLine
    {
        private static readonly IConfiguration Configuration;

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
        public static Task Literal(
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
            [Required]string destinationExpression,
            string? adjacencyAttribute = null,
            int? httpRequestRetries = null,
            TimeSpan? httpRequestDelayBetweenRetries = null,
            bool whatIf = true,
            string? rootUrl = null,
            bool fullScan = false)
        {
            var appService = BuildScrapperApplicationService(
                Configuration,
                fullScan);

            return appService.ScrapAsync(
                new JobDefinitionDto(
                    adjacencyXPath,
                    adjacencyAttribute ?? "href",
                    resourceXPath,
                    resourceAttribute,
                    new FileSystemResourceRepositoryConfiguration(
                        destinationExpression,
                        destinationRootFolder),
                    rootUrl,
                    httpRequestRetries,
                    httpRequestDelayBetweenRetries,
                    whatIf));
        }

        [Verb(Description = "Adds to the database a job definition provided with the arguments")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static Task Add(
            [Required]string name,
            [Required]string adjacencyXPath,
            [Required]string resourceXPath,
            [Required]string resourceAttribute,
            [Required]string destinationRootFolder,
            [Required]string destinationExpression,
            string? adjacencyAttribute = null,
            int? httpRequestRetries = null,
            TimeSpan? httpRequestDelayBetweenRetries = null,
            string? rootUrl = null)
        {
            var defsAppService = BuildJobDefinitionsApplicationService(Configuration);
            
            var jobDefinition = new JobDefinitionDto(
                adjacencyXPath,
                adjacencyAttribute ?? "href",
                resourceXPath,
                resourceAttribute,
                new FileSystemResourceRepositoryConfiguration(
                    destinationExpression,
                    destinationRootFolder),
                rootUrl,
                httpRequestRetries,
                httpRequestDelayBetweenRetries,
                false);

            jobDefinition.Log(LoggerFactoryInstance.CreateLogger("Console"));
            
            return defsAppService.AddJobAsync(
                name,
                jobDefinition);
        }

        [Verb(Aliases = "del", Description = "Deletes a job definition from the database")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static Task Delete(string name)
        {
            var defsAppService = BuildJobDefinitionsApplicationService(Configuration);
            
            return defsAppService.DeleteJobAsync(name);
        }

        [Verb(Description = "Shows a job definition from the database")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static async Task Show(string name)
        {
            var defsAppService = BuildJobDefinitionsApplicationService(Configuration);
            
            var jobDefinition = await defsAppService.GetJobAsync(name);
            jobDefinition.Log(LoggerFactoryInstance.CreateLogger("Console"));
        }

        [Verb(IsDefault = true, Description = "Executes a job definition from the database")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static async Task Scrap(
            string? name = null,
            string? rootUrl = null,
            bool? whatIf = false,
            bool fullScan = false)
        {
            var defsAppService = BuildJobDefinitionsApplicationService(Configuration);
            JobDefinitionDto scrapJobDefinition;
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
                scrapJobDefinition = scrapJobDefinition with { RootUrl = rootUrl ?? scrapJobDefinition.RootUrl };
            
                if (scrapJobDefinition.RootUrl == null)
                {
                    throw new Exception("No Root URL found as argument or in the job definition");
                }
            }

            var scrapAppService = BuildScrapperApplicationService(Configuration, fullScan);
            
            await scrapAppService.ScrapAsync(scrapJobDefinition with { WhatIf = whatIf ?? scrapJobDefinition.WhatIf });            
        }

        [PostVerbExecution]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static void After(PostVerbExecutionContext context)
        {
            Console.WriteLine(context.Exception);
        }
    }
}