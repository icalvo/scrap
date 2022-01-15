using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CLAP;
using CLAP.Interception;
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
            var scrapAppService = serviceResolver.BuildScrapperApplicationService();
            await scrapAppService.RunAsync(newJob);
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
