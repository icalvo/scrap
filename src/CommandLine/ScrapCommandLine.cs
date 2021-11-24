using System;
using System.Diagnostics;
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
        private bool _verbose;
        private bool _debug;
        private IConfiguration _configuration = null!;
        private ILoggerFactory _loggerFactory = null!;
        private ILogger<ScrapCommandLine> _logger = null!;

        [PreVerbExecution]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        void Before(PreVerbExecutionContext context)
        {
            if (_debug)
            {
                Debugger.Launch();
            }

            _configuration =
                new ConfigurationBuilder()
                    .AddJsonFile("scrap.json", optional: false)
                    .Build();

            _loggerFactory = LoggerFactory.Create(builder =>
            {
                if (_verbose)
                {
                    builder.SetMinimumLevel(LogLevel.Trace);
                }
                else
                {
                    builder.AddConfiguration(_configuration.GetSection("Logging"));
                }

                builder.AddSimpleConsole(options => options.SingleLine = true);
            });

            _logger = new Logger<ScrapCommandLine>(_loggerFactory);
            _logger.LogInformation("Scrap DB: {ConnectionString}", _configuration["Scrap:Database"]);
        }

        [Global(Aliases="dbg")]
        public void Debug()
        {
            _debug = true;
        }

        [Global(Aliases="v")]
        public void Verbose()
        {
            _verbose = true;
        }

        [Verb(IsDefault = true, Description = "Executes a job definition from the database")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public async Task Scrap(
            string? name = null,
            string? rootUrl = null,
            bool whatIf = false,
            bool fullScan = false,
            bool async = false)
        {
            var serviceResolver = new ServicesResolver(_loggerFactory, _configuration);
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
                _logger.LogInformation("Hangfire job created with Id: {JobId}", jobId);
            }
            else
            {
                await scrapAppService.RunAsync(newJob);
            }
        }


        [Verb(Description = "Executes a job definition from the database")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public async Task Resources(
            string? name = null,
            string? rootUrl = null,
            bool whatIf = false,
            bool fullScan = false,
            bool async = false)
        {
            var serviceResolver = new ServicesResolver(_loggerFactory, _configuration);
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
                var jobStorage = new SqlServerStorage(_configuration["Hangfire:Database"]);
                var client = new BackgroundJobClient(jobStorage);

                var jobId = client.Create(() => scrapAppService.ListResourcesAsync(newJob), new EnqueuedState());
                _logger.LogInformation("Hangfire job created with Id: {JobId}", jobId);
            }
            else
            {
                await scrapAppService.ListResourcesAsync(newJob);
            }
        }
        
        [Verb(Description = "Lists jobs")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public Task List()
        {
            var jobStorage = new SqlServerStorage(_configuration["Hangfire:Database"]);
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
        public Task Cancel(string jobId)
        {
            var jobStorage = new SqlServerStorage(_configuration["Hangfire:Database"]);
            var client = new BackgroundJobClient(jobStorage);
            client.Delete(jobId);
            _logger.LogInformation("Hangfire job deleted with Id: {JobId}", jobId);

            return Task.CompletedTask;
        }

        [PostVerbExecution]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public void After(PostVerbExecutionContext context)
        {
            if (context.Failed)
            {
                _logger.LogCritical(context.Exception, "Critical error");
            }

            if (_debug)
            {
                Console.ReadKey();
            }
        }
    }
}
