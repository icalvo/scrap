using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CLAP;
using CLAP.Interception;
using Figgle;
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
        if (!context.Method.Names.Contains("configure"))
        {
            EnsureGlobalConfiguration(globalUserConfigFolder, globalUserConfigPath);
        }

        _configuration =
            new ConfigurationBuilder()
                .AddJsonFile("scrap.json", optional: false, reloadOnChange: false)
                .AddJsonFile(globalUserConfigPath, optional: false, reloadOnChange: false)
                .Build();
            
        SetupLogging(_verbose ? LogLevel.Trace : null);
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
        [Description("Job definition name"),Aliases("n")]string? name = null,
        [Description("URL where the scrapping starts"),Aliases("r")]string? rootUrl = null,
        [Description("Starts all the job definitions with a root URL set"),Aliases("a")]bool all = false,
        [Description("Navigate through already visited pages"),Aliases("f")]bool fullScan = false,
        [Description("Download resources even if they are already downloaded"),Aliases("d")]bool downloadAlways = false,
        [Description("Disable mark as visited"),Aliases("dmv")]bool disableMarkingVisited = false,
        [Description("Disable writing the resource"),Aliases("dwr")]bool disableResourceWrites = false
        )
    {
        PrintHeader();

        var serviceResolver = new ServicesResolver(_loggerFactory, _configuration);
        var definitionsApplicationService = await serviceResolver.BuildJobDefinitionsApplicationServiceAsync();
        
        var jobDefs = new List<JobDefinitionDto>();
        var envRootUrl = Environment.GetEnvironmentVariable("JOBDEF_ROOT_URL");
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
        else
        {
            var envName = Environment.GetEnvironmentVariable("JOBDEF_NAME");
            var jobDef = await GetJobDefinition(name, rootUrl, definitionsApplicationService, envName, envRootUrl);

            if (jobDef == null)
            {
                _logger.LogWarning("No job definition found, nothing will be done");
                return;
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
            var newJob = new NewJobDto(jobDef, rootUrl ?? envRootUrl, fullScan, null, downloadAlways, disableMarkingVisited, disableResourceWrites);
            var scrapAppService = serviceResolver.BuildScrapperApplicationService();
            await scrapAppService.ScrapAsync(newJob);
        }
    }

    [Verb(Description = "Lists all the pages reachable with the adjacency path")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public async Task Traverse(
        [Description("Job definition name")]string? name = null,
        [Description("URL where the scrapping starts")]string? rootUrl = null,
        [Description("Navigate through already visited pages")]bool fullScan = false)
    {
        if (!_verbose)
        {
            SetupLogging(LogLevel.Error);
        }
        
        var serviceResolver = new ServicesResolver(_loggerFactory, _configuration);
        var newJob = await BuildJobDto(serviceResolver, name, rootUrl,  fullScan, downloadAlways: false, disableMarkingVisited: true, disableResourceWrites: true);
        if (newJob == null)
        {
            return;
        }

        var scrapAppService = serviceResolver.BuildScrapperApplicationService();
        await scrapAppService.TraverseAsync(newJob).ForEachAsync(x => Console.WriteLine(x));
    }

    [Verb(Description = "Lists all the pages reachable with the adjacency path")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public async Task Resources(
        [Description("Job definition name")]string? name = null,
        [Description("URL where the scrapping starts")]string? rootUrl = null,
        [Description("Pipeline")]string[]? pipeline = null,
        [Description("Output only the resource link instead of the format expected by 'scrap download'")]bool onlyResourceLink = false)
    {
        if (!_verbose)
        {
            SetupLogging(LogLevel.Error);
        }

        var serviceResolver = new ServicesResolver(_loggerFactory, _configuration);
        var newJob = await BuildJobDto(
            serviceResolver,
            name,
            rootUrl,
            fullScan: false,
            downloadAlways: false,
            disableMarkingVisited: true,
            disableResourceWrites: true);
        if (newJob == null)
        {
            return;
        }

        var scrapAppService = serviceResolver.BuildScrapperApplicationService();
        var pageIndex = 0;
        IEnumerable<string> inputLines = pipeline ?? ConsoleInput();
        foreach (var line in inputLines)
        {
            var pageUrl = new Uri(line);
            await scrapAppService.GetResourcesAsync(newJob, pageUrl, pageIndex).ForEachAsync((resourceUrl, resourceIndex) =>
            {
                var format = onlyResourceLink ? "{3}" : "{0} {1} {2} {3}";
                Console.WriteLine(format, pageIndex, pageUrl, resourceIndex, resourceUrl);
            });
            pageIndex++;
        }
    }

    [Verb(Description = "Lists all the pages reachable with the adjacency path")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public async Task Download(
        [Description("Job definition name")]string? name = null,
        [Description("URL where the scrapping starts")]string? rootUrl = null,
        [Description("Download resources even if they are already downloaded")]bool downloadAlways = false,
        [Description("Pipeline")]string[]? pipeline = null)
    {
        if (!_verbose)
        {
            SetupLogging(LogLevel.Error);
        }

        var serviceResolver = new ServicesResolver(_loggerFactory, _configuration);
        var newJob = await BuildJobDto(
            serviceResolver,
            name,
            rootUrl,
            fullScan: false,
            downloadAlways,
            disableMarkingVisited: true,
            disableResourceWrites: false);
        if (newJob == null)
        {
            return;
        }

        var scrapAppService = serviceResolver.BuildScrapperApplicationService();
        IEnumerable<string> inputLines = pipeline ?? ConsoleInput();
        foreach (var line in inputLines)
        {
            var split = line.Split(" ");
            var pageIndex = int.Parse(split[0]);
            var pageUrl = new Uri(split[1]);
            var resourceIndex = int.Parse(split[2]);
            var resourceUrl = new Uri(split[3]);
            await scrapAppService.DownloadAsync(newJob, pageUrl, pageIndex, resourceUrl, resourceIndex);
            Console.WriteLine("Downloaded " + resourceUrl);
        }
    }

    [Verb(Description = "Lists all the pages reachable with the adjacency path")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public async Task MarkVisited(
        [Description("Job definition name")]string? name = null,
        [Description("URL where the scrapping starts")]string? rootUrl = null,
        [Description("Pipeline")]string[]? pipeline = null)
    {
        if (!_verbose)
        {
            SetupLogging(LogLevel.Error);
        }
        
        var serviceResolver = new ServicesResolver(_loggerFactory, _configuration);
        var newJob = await BuildJobDto(
            serviceResolver,
            name,
            rootUrl,
            fullScan: false,
            downloadAlways: false,
            disableMarkingVisited: true,
            disableResourceWrites: false);
        if (newJob == null)
        {
            return;
        }

        var scrapAppService = serviceResolver.BuildScrapperApplicationService();
        IEnumerable<string> inputLines = pipeline ?? ConsoleInput();
        foreach (var line in inputLines)
        {
            var pageUrl = new Uri(line);
            await scrapAppService.MarkVisitedPageAsync(newJob, pageUrl);
            Console.WriteLine("Visited " + pageUrl);
        }
    }

    [Verb(Description = "Configures the tool", Aliases = "c,config")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Configure()
    {
        PrintHeader();

        var globalUserConfigFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".scrap");
        var globalUserConfigPath = Path.Combine(globalUserConfigFolder, "scrap-user.json");
        
        Directory.CreateDirectory(globalUserConfigFolder);
        if (File.Exists(globalUserConfigPath))
        {
            Console.WriteLine($"Global config file found at: {globalUserConfigPath}");
        }
        else
        {
            Console.WriteLine(
                $"Global config file not found. We are going to create a global config file and ask some values. " +
                "This file is located at: {globalUserConfigPath}");
            Console.WriteLine(
                $"The global config file will not be modified or deleted by any install, update or uninstall of this tool.");
            File.WriteAllText(globalUserConfigPath, "{ \"Scrap\": {}}");
            Console.WriteLine($"Created global config at: {globalUserConfigPath}");
        }

        var cfg =
            new ConfigurationBuilder()
                .AddJsonFile(globalUserConfigPath, optional: false, reloadOnChange: false)
                .Build();
        SetUpGlobalConfigValues(globalUserConfigFolder, globalUserConfigPath, cfg);
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

    private record GlobalConfig(
        string Key,
        string DefaultValue,
        string Prompt);

    private static void EnsureGlobalConfiguration(string globalUserConfigFolder, string globalUserConfigPath)
    {
        if (!File.Exists(globalUserConfigPath))
        {
            throw new Exception("The tool is not properly configured; call 'scrap config'.");
        }

        var cfg =
            new ConfigurationBuilder()
                .AddJsonFile(globalUserConfigPath, optional: false, reloadOnChange: false)
                .Build();
        if (GetGlobalConfigs(globalUserConfigFolder).Any(config => cfg[config.Key] == null))
        {
            throw new Exception("The tool is not properly configured; call 'scrap config'.");
        }
    }

    private static IEnumerable<GlobalConfig> GetGlobalConfigs(string globalUserConfigFolder) => new []{
        new GlobalConfig(
            "Scrap:Definitions",
            Path.Combine(globalUserConfigFolder, "jobDefinitions.json"),
            "Path for job definitions JSON"),
        new GlobalConfig(
            "Scrap:Database",
            $"Filename={Path.Combine(globalUserConfigFolder, "scrap.db")};Connection=shared",
            "Connection string for page markings LiteDB database")
    };

    private static void SetUpGlobalConfigValues(
        string globalUserConfigFolder,
        string globalUserConfigPath,
        IConfiguration cfg)
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
        
        var updates =
            GetGlobalConfigs(globalUserConfigFolder)
                .Select(EnsureGlobalConfigValue)
                .RemoveNulls()
                .ToArray();
        if (updates.Length == 0)
        {
            Console.WriteLine("Nothing changed!");
        }
        else
        {
            Console.WriteLine($"Adding or updating {updates.Length} config value(s)");
            var updater = new JsonUpdater(globalUserConfigPath);
            updater.AddOrUpdate(updates);
        }
            
        KeyValuePair<string, object>? EnsureGlobalConfigValue(GlobalConfig globalConfig)
        {
            var (key, defaultValue, prompt) = globalConfig;
            if (cfg[key] != null)
            {
                defaultValue = cfg[key];
            }

            Console.Write($"{prompt} [{defaultValue}]: ");
            var value = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(value))
            {
                value = defaultValue;
            }

            if (value == cfg[key])
            {
                return null;
            }

            return new KeyValuePair<string, object>(key, value);
        }
    }

    private void SetupLogging(LogLevel? minimumLevel)
    {
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            if (minimumLevel.HasValue)
            {
                builder.SetMinimumLevel(minimumLevel.Value);
            }
            else
            {
                builder.AddConfiguration(_configuration.GetSection("Logging"));
            }

            builder.AddConsole(options =>
            {
                options.LogToStandardErrorThreshold = LogLevel.Warning;
            });
        });

        _logger = new Logger<ScrapCommandLine>(_loggerFactory);
    }

    private static void PrintHeader()
    {
        var currentColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(FiggleFonts.Standard.Render("SCRAP"));
        var version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
        Console.WriteLine("Command line tool for generic web scrapping, version " + version);
        Console.ForegroundColor = currentColor;
    }

    private static IEnumerable<string> ConsoleInput()
    {
        string? line;
        while ((line = Console.ReadLine()) != null)
        {
            yield return line;
        }
    }

    private async Task<NewJobDto?> BuildJobDto(
        ServicesResolver serviceResolver,
        string? name,
        string? rootUrl,
        bool? fullScan,
        bool? downloadAlways,
        bool? disableMarkingVisited,
        bool? disableResourceWrites)
    {
        var definitionsApplicationService = await serviceResolver.BuildJobDefinitionsApplicationServiceAsync();
        var envName = Environment.GetEnvironmentVariable("JOBDEF_NAME");
        var envRootUrl = Environment.GetEnvironmentVariable("JOBDEF_ROOT_URL");
        
        var jobDef = await GetJobDefinition(name, rootUrl, definitionsApplicationService, envName, envRootUrl);

        if (jobDef == null)
        {
            _logger.LogWarning("No job definition found, nothing will be done");
            return null;
        }
        
        _logger.LogInformation("The following job def will be run: {JobDef}", jobDef);
        
        return new NewJobDto(jobDef, rootUrl ?? envRootUrl, fullScan, null, downloadAlways, disableMarkingVisited, disableResourceWrites);        
    }

    private async Task<JobDefinitionDto?> GetJobDefinition(
        string? name,
        string? rootUrl,
        JobDefinitionsApplicationService definitionsApplicationService,
        string? envName,
        string? envRootUrl)
    {
        JobDefinitionDto? jobDef = null;
        if (name != null)
        {
            jobDef = await definitionsApplicationService.FindJobByNameAsync(name);
            if (jobDef == null)
            {
                _logger.LogWarning("Job definition {Name} does not exist", name);
            }

            return jobDef;
        }
        
        if (rootUrl != null)
        {
            jobDef = await definitionsApplicationService.FindJobByRootUrlAsync(rootUrl);
            if (jobDef == null)
            {
                _logger.LogWarning("No job definition matches with {RootUrl}", rootUrl);
            }
            else
            {
                return jobDef;
            }
        }
        
        if (envName != null)
        {
            jobDef = await definitionsApplicationService.FindJobByNameAsync(envName);
            if (jobDef == null)
            {
                _logger.LogWarning("Job definition {Name} does not exist", envName);
            }

            return jobDef;
        }
        
        if (envRootUrl != null)
        {
            jobDef = await definitionsApplicationService.FindJobByRootUrlAsync(envRootUrl);
            if (jobDef == null)
            {
                _logger.LogWarning("No job definition matches with {RootUrl}", envRootUrl);
            }
        }

        return jobDef;
    }
}
