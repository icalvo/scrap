using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CLAP;
using CLAP.Interception;
using Hangfire;
using Hangfire.States;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scrap.JobDefinitions;
using Scrap.Jobs;
using Scrap.DependencyInjection;

namespace Scrap.CommandLine;

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

        var globalUserConfigFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".scrap");
        var globalUserConfigPath = Path.Combine(globalUserConfigFolder, "scrap-user.json");

        EnsureGlobalConfiguration(globalUserConfigFolder, globalUserConfigPath);

        _configuration =
            new ConfigurationBuilder()
                .AddJsonFile("scrap.json", optional: false, reloadOnChange: false)
                .AddJsonFile(globalUserConfigPath, optional: false, reloadOnChange: false)
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
    }

    [Global(Aliases="dbg", Description = "Runs a debugger session at the beginning")]
    public void Debug()
    {
        _debug = true;
    }

    [Global(Aliases="v", Description = "Verbose output")]
    public void Verbose()
    {
        _verbose = true;
    }

    [Verb(IsDefault = true, Description = "Executes a job definition from the database")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public async Task Scrap(
        [Description("Job definition name")]string? name = null,
        [Description("URL where the scrapping starts")]string? rootUrl = null,
        [Description("Starts all the job definitions with a root URL set")]bool all = false,
        [Description("Do everything except actually downloading resources")]bool whatIf = false,
        [Description("Navigate through already visited pages")]bool fullScan = false,
        [Description("Launch the job asynchronously")]bool async = false,
        [Description("Download resources even if they are already downloaded")]bool downloadAlways = false)
    {
        if (!all && name == null && rootUrl == null)
        {
            throw new ArgumentException($"At least one of these options must be present: '{nameof(all)}', '{nameof(name)}', '{nameof(rootUrl)}'");
        }

        var serviceResolver = new ServicesResolver(_loggerFactory, _configuration);
        var definitionsApplicationService = await serviceResolver.BuildJobDefinitionsApplicationServiceAsync();
        var jobDefs = new List<JobDefinitionDto>();
        if (all)
        {
            if (name != null || rootUrl != null)
            {
                throw new ArgumentException($"'{nameof(all)}' switch is incompatible with '{nameof(name)}' or '{nameof(rootUrl)}' options");
            }

            await foreach (var jobDef in definitionsApplicationService.GetJobsAsync().Where(x => x.RootUrl != null))
            {
                jobDefs.Add(jobDef);
            }
        }
        else if (name != null)
        {
            var jobDef = await definitionsApplicationService.FindJobByNameAsync(name);
            if (jobDef != null)
            {
                jobDefs.Add(jobDef);
            }
        }
        else if (rootUrl != null)
        {
            var jobDef = await definitionsApplicationService.FindJobByRootUrlAsync(rootUrl);
            if (jobDef != null)
            {
                jobDefs.Add(jobDef);
            }
        }

        if (jobDefs.Count == 0)
        {
            _logger.LogWarning("No job definition found, nothing will be done");
            return;
        }

        _logger.LogInformation("The following job def(s). will be run: {JobDefs}", string.Join(", ", jobDefs.Select(x => x.Name)));
        foreach (var jobDef in jobDefs)
        {
            var newJob = new NewJobDto(jobDef, rootUrl, whatIf, fullScan, null, downloadAlways);
            await ExecuteJob(newJob, async, serviceResolver);
        }
    }

    [Verb(Description = "Navigates the site and lists the resources that will be downloaded")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public async Task Resources(
        [Description("Job definition name")]string? name = null,
        [Description("URL where the scrapping starts")]string? rootUrl = null,
        [Description("Do everything except actually downloading resources")]bool whatIf = false,
        [Description("Navigate through already visited pages")]bool fullScan = false)
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

        var newJob = new NewJobDto(jobDef, rootUrl, whatIf, fullScan, null, false);
        await scrapAppService.ListResourcesAsync(newJob);
    }

    [Verb(Description = "Lists jobs being executed asynchronously")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public Task List()
    {
        var serviceResolver = new ServicesResolver(_loggerFactory, _configuration);
        var jobStorage = serviceResolver.BuildHangfireJobStorage();
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
    public Task Cancel(
        [Description("Navigate through already visited pages")]string jobId)
    {
        var serviceResolver = new ServicesResolver(_loggerFactory, _configuration);
        var client = serviceResolver.BuildHangfireBackgroundJobClient();
        client.Delete(jobId);
        _logger.LogInformation("Hangfire job deleted with Id: {JobId}", jobId);

        return Task.CompletedTask;
    }

    private async Task ExecuteJob(NewJobDto newJob, bool async, ServicesResolver serviceResolver)
    {
        var scrapAppService = serviceResolver.BuildScrapperApplicationService();
        if (async)
        {
            IBackgroundJobClient? client = serviceResolver.BuildHangfireBackgroundJobClient();

            var jobId = client.Create(() => scrapAppService.RunAsync(newJob), new EnqueuedState());
            _logger.LogInformation("Hangfire job created with Id: {JobId}", jobId);
        }
        else
        {
            await scrapAppService.RunAsync(newJob);
        }
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

    private static void EnsureGlobalConfiguration(string globalUserConfigFolder, string globalUserConfigPath)
    {
        Directory.CreateDirectory(globalUserConfigFolder);
        if (!File.Exists(globalUserConfigPath))
        {
            Console.WriteLine($"Global config file not found. We are going to create a global config file and ask some values. " +
                              "This file is located at: {globalUserConfigPath}");
            Console.WriteLine($"The global config file will not be modified or deleted by any install, update or uninstall of this tool.");
            File.WriteAllText(globalUserConfigPath, "{ \"Scrap\": {}}");
            Console.WriteLine("Created global config at: " + globalUserConfigPath);
        }

        var cfg =
            new ConfigurationBuilder()
                .AddJsonFile(globalUserConfigPath, optional: false, reloadOnChange: false)
                .Build();
        EnsureGlobalConfigValues(globalUserConfigFolder, globalUserConfigPath, cfg);
    }

    private static void EnsureGlobalConfigValues(
        string globalUserConfigFolder,
        string globalUserConfigPath,
        IConfiguration cfg)
    {
        var updates = new[]
        {
            EnsureGlobalConfigValue(
                "Scrap:Definitions",
                Path.Combine(globalUserConfigFolder, "jobDefinitions.json"),
                "Path for job definitions JSON"),
            EnsureGlobalConfigValue(
                "Scrap:Database",
                Path.Combine(globalUserConfigFolder, "scrap.db"),
                "Path for  database",
                "Filename={0};Connection=shared")
        }.RemoveNulls();
        var updater = new JsonUpdater(globalUserConfigPath);
        updater.AddOrUpdate(updates);
            
        KeyValuePair<string, object>? EnsureGlobalConfigValue(
            string key,
            string defaultValue,
            string prompt,
            string valueFormat = "{0}"
        )
        {
            if (cfg[key] != null)
            {
                return null;
            }

            Console.Write($"{prompt} [{defaultValue}]: ");
            var value = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(value))
            {
                value = defaultValue;
            }

            return new KeyValuePair<string, object>(key, string.Format(valueFormat, value));
        }
    }
}
