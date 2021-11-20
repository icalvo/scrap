using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CLAP;
using CLAP.Interception;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.States;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scrap.JobDefinitions;
using Scrap.Jobs;
using Scrap.DependencyInjection;

namespace Scrap.CommandLine
{
    public class ScrapCommandLine
    {
        private static readonly IConfiguration Configuration;
        private static readonly ILoggerFactory LoggerFactoryInstance;

        static ScrapCommandLine()
        {

            Configuration =
                new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false)
                    .Build();

            LoggerFactoryInstance = LoggerFactory.Create(builder =>
            {
                builder
                    .AddConfiguration(Configuration.GetSection("Logging"))
                    .AddSimpleConsole(options => options.SingleLine = true);
            });
            
            Console.WriteLine("Scrap DB: {0}", Configuration["Scrap:Database"]);
        }

        [Verb(IsDefault = true, Description = "Executes a job definition from the database")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static async Task Scrap(
            string? name = null,
            string? rootUrl = null,
            bool whatIf = false,
            bool fullScan = false,
            bool async = false)
        {
            var serviceResolver = new ServicesResolver(LoggerFactoryInstance, Configuration);
            var definitionsApplicationService = await serviceResolver.BuildJobDefinitionsApplicationServiceAsync();
            var scrapAppService = serviceResolver.BuildScrapperApplicationService();
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
            if (async)
            {
                var jobStorage = new SqlServerStorage("Data Source=(localdb)\\MSSQLLocalDB");
                var client = new BackgroundJobClient(jobStorage);

                var jobId = client.Create(() => scrapAppService.RunAsync(newJob), new EnqueuedState());
                Console.WriteLine("Job created with Id: " + jobId);
            }
            else
            {
                await scrapAppService.RunAsync(newJob);
            }
        }


        [Verb(Description = "Executes a job definition from the database")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static async Task Resources(
            string? name = null,
            string? rootUrl = null,
            bool whatIf = false,
            bool fullScan = false,
            bool async = false)
        {
            var serviceResolver = new ServicesResolver(LoggerFactoryInstance, Configuration);
            var definitionsApplicationService = await serviceResolver.BuildJobDefinitionsApplicationServiceAsync();
            var scrapAppService = serviceResolver.BuildScrapperApplicationService();
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
            if (async)
            {
                var jobStorage = new SqlServerStorage("Data Source=(localdb)\\MSSQLLocalDB");
                var client = new BackgroundJobClient(jobStorage);

                var jobId = client.Create(() => scrapAppService.RunAsync(newJob), new EnqueuedState());
                Console.WriteLine("Job created with Id: " + jobId);
            }
            else
            {
                await scrapAppService.RunAsync(newJob);
            }
        }
        
        [Verb(Description = "Lists jobs")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static Task List()
        {
            var jobStorage = new SqlServerStorage("Data Source=(localdb)\\MSSQLLocalDB");
            var monitoringApi = jobStorage.GetMonitoringApi();

            monitoringApi.ScheduledJobs(0, 100).ForEach(
                job =>
                {
                    var (key, value) = job;
                    Console.WriteLine($"JOB {key}:");
                    Console.WriteLine($"  Status: Scheduled");
                    Console.WriteLine($"  Enqueued: {value.EnqueueAt}");
                    Console.WriteLine($"  Scheduled: {value.ScheduledAt}");
                },
                () => Console.WriteLine("No scheduled jobs"));

            var jobsInQueue =
                from queue in monitoringApi.Queues()
                from job in queue.FirstJobs
                where job.Value.State != "Deleted"
                select (queue.Name, job.Key, job.Value);

            jobsInQueue.ForEach(x =>
                {
                    var (queue, jobId, job) = x;
                    Console.WriteLine($"JOB {jobId}:");
                    Console.WriteLine($"  Status: Enqueued");
                    Console.WriteLine($"  Queue: {queue}");
                    Console.WriteLine($"  Enqueued: {job.EnqueuedAt}");
                    Console.WriteLine($"  State: {job.State}");
                    var details = monitoringApi.JobDetails(jobId);
                    foreach (var (propKey, propValue) in details.Properties)
                    {
                        Console.WriteLine($"  {propKey}: {propValue}");
                    }
                },
                () => Console.WriteLine("No enqueued jobs"));

            monitoringApi.SucceededJobs(0, 100).ForEach(x =>
                {
                    var (jobId, job) = x;
                    TimeSpan? duration = job.TotalDuration == null
                        ? null
                        : TimeSpan.FromMilliseconds(job.TotalDuration.Value);
                    Console.WriteLine($"JOB {jobId}:");
                    Console.WriteLine($"  Status: Succeeded");
                    Console.WriteLine($"  Succeeded: {job.SucceededAt}");
                    Console.WriteLine($"  Result: {job.Result}");
                    Console.WriteLine($"  Duration: {duration}");
                    var details = monitoringApi.JobDetails(jobId);
                    Console.WriteLine($"  History:");
                    foreach (var historyDto in details.History)
                    {
                        Console.WriteLine($"    Changed: {historyDto.CreatedAt}");
                        Console.WriteLine($"    State: {historyDto.StateName}");
                        Console.WriteLine($"    Reason: {historyDto.Reason}");
                        
                    }
                },
                () => Console.WriteLine("No succeeded jobs"));

            return Task.CompletedTask;
        }

        [Verb(Description = "Cancels a job")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static Task Cancel(string jobId)
        {
            var jobStorage = new SqlServerStorage("Data Source=(localdb)\\MSSQLLocalDB");
            var client = new BackgroundJobClient(jobStorage);
            client.Delete(jobId);
            Console.WriteLine("Job deleted with Id: " + jobId);

            return Task.CompletedTask;
        }

        [PostVerbExecution]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static void After(PostVerbExecutionContext context)
        {
            Console.WriteLine(context.Exception);
        }
    }
}
