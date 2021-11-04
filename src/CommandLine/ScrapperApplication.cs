using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CLAP;
using CLAP.Interception;
using Scrap.JobDefinitions;
using static Scrap.CommandLine.DependencyInjection;

namespace Scrap.CommandLine
{
    public class ScrapperApplication
    {
        private const int DefaultHttpRequestRetries = 5;
        private static readonly TimeSpan DefaultHttpRequestDelayBetweenRetries = TimeSpan.FromSeconds(1);
        
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
                httpRequestRetries ?? DefaultHttpRequestRetries,
                httpRequestDelayBetweenRetries ?? DefaultHttpRequestDelayBetweenRetries,
                fullScan);

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

            jobDefinition.Log(LoggerFactoryInstance.CreateLogger("Console"));
            
            return defsAppService.AddJobAsync(
                name,
                jobDefinition);
        }

        [Verb(Aliases = "del", Description = "Deletes a job definition from the database")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static Task Delete(string name)
        {
            var defsAppService = BuildJobDefinitionsApplicationService();
            
            return defsAppService.DeleteJobAsync(name);
        }

        [Verb(Description = "Shows a job definition from the database")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static async Task Show(string name)
        {
            var defsAppService = BuildJobDefinitionsApplicationService();
            
            var jobDefinition = await defsAppService.GetJobAsync(name);
            jobDefinition.Log(LoggerFactoryInstance.CreateLogger("Console"));
        }

        [Verb(IsDefault = true, Description = "Executes a job definition from the database")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static async Task Scrap(
            string? name = null,
            string? rootUrl = null,
            bool whatIf = true,
            bool fullScan = false)
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
                scrapJobDefinition.HttpRequestDelayBetweenRetries,
                fullScan);
            
            await scrapAppService.ScrapAsync(scrapJobDefinition, whatIf);            
        }

        [PostVerbExecution]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static void After(PostVerbExecutionContext context)
        {
            Console.WriteLine(context.Exception);
        }
    }
}