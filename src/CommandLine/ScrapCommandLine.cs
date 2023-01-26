using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CLAP;
using CLAP.Interception;
using Figgle;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scrap.Application;
using Scrap.Application.Scrap;
using Scrap.CommandLine.Logging;
using Scrap.DependencyInjection;
using Scrap.Domain;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Jobs;

namespace Scrap.CommandLine;

public class ScrapCommandLine
{
    private const string JobDefNameEnvironment = "JobDefinition:DefaultName";
    private const string JobDefRootUrlEnvironment = "JobDefinition:DefaultRootUrl";
    private const string ConfigFolderEnvironment = "GlobalConfigurationFolder";
    private bool _verbose;
    private bool _debug;
    private IConfiguration _configuration = null!;

    public ScrapCommandLine(Parser<ScrapCommandLine> parser, string[] args)
    {
        parser.Register.HelpHandler("help,h,?", Help);
        parser.Register.ErrorHandler((Action<ExceptionContext>) (c => Exception(c, args)));
    }

    [PreVerbExecution]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    void Before(PreVerbExecutionContext context)
    {
        if (_debug)
        {
            Debugger.Launch();
        }

        const string environmentVarPrefix = "Scrap_";
        _configuration = new ConfigurationBuilder().AddEnvironmentVariables(environmentVarPrefix).Build();
        var globalUserConfigFolder = GetGlobalUserConfigFolder();
        var globalUserConfigPath = Path.Combine(globalUserConfigFolder, "scrap-user.json");


        var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("scrap.json", optional: false, reloadOnChange: false);
        if (!context.Method.Names.Contains("configure"))
        {
            if (!File.Exists(globalUserConfigPath))
            {
                Console.WriteLine("The tool is not configured, please run 'scrap configure'.");
                context.Cancel = true;
                return;
            }

            _ = configBuilder.AddJsonFile(globalUserConfigPath, optional: false, reloadOnChange: false);
        }
 
        _configuration = configBuilder.AddEnvironmentVariables(prefix: environmentVarPrefix).Build();

        if (!context.Method.Names.Contains("configure"))
        {
            EnsureGlobalConfiguration(globalUserConfigFolder, globalUserConfigPath);
        }
    }

    [Global(Aliases="dbg", Description = "Runs a debugger session at the beginning")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public void Debug()
    {
        _debug = true;
    }

    [Global(Aliases="v", Description = "Verbose output")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
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

        var serviceResolver = new ServicesLocator(_configuration, ConfigureLoggingWithConsole);
        var logger = serviceResolver.Get<ILogger<ScrapCommandLine>>();
        var definitionsApplicationService = serviceResolver.Get<JobDefinitionsApplicationService>();
    
        var jobDefs = new List<JobDefinitionDto>();
        var envRootUrl = _configuration[JobDefRootUrlEnvironment];
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
            var envName = _configuration[JobDefNameEnvironment];
            var jobDef = await GetJobDefinition(name, rootUrl, definitionsApplicationService, envName, envRootUrl, logger);

            if (jobDef == null)
            {
                return;
            }

            jobDefs.Add(jobDef);
        }

        if (jobDefs.Count == 0)
        {
            logger.LogWarning("No job definition found, nothing will be done");
            return;
        }

        logger.LogInformation("The following job def(s). will be run: {JobDefs}", string.Join(", ", jobDefs.Select(x => x.Name)));
        foreach (var jobDef in jobDefs)
        {
            var newJob = new JobDto(jobDef, rootUrl ?? envRootUrl, fullScan, null, downloadAlways, disableMarkingVisited, disableResourceWrites);
            var scrapAppService = serviceResolver.Get<ScrapApplicationService>();
            logger.LogInformation("Starting {Definition}...", jobDef.Name);
            await scrapAppService.ScrapAsync(newJob);
            logger.LogInformation("Finished!");
        }
    }

    [Verb(Description = "Lists all the pages reachable with the adjacency path", Aliases = "t")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public async Task Traverse(
        [Description("Job definition name")]string? name = null,
        [Description("URL where the scrapping starts")]string? rootUrl = null,
        [Description("Navigate through already visited pages")]bool fullScan = false)
    {
        var serviceResolver = new ServicesLocator(_configuration, ConfigureLoggingWithoutConsole);
        var newJob = await BuildJobDto(serviceResolver, name, rootUrl,  fullScan, downloadAlways: false, disableMarkingVisited: true, disableResourceWrites: true);
        if (newJob == null)
        {
            return;
        }

        var service = serviceResolver.Get<ITraversalApplicationService>();
        await service.TraverseAsync(newJob).ForEachAsync(x => Console.WriteLine(x));
    }

    [Verb(Description = "Lists all the resources available in pages provided by console input", Aliases = "r")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public async Task Resources(
        [Description("Job definition name")]string? name = null,
        [Description("URL where the scrapping starts")]string? rootUrl = null,
        [Description("Pipeline")]string[]? pipeline = null,
        [Description("Output only the resource link instead of the format expected by 'scrap download'")]bool onlyResourceLink = false)
    {
        var serviceResolver = new ServicesLocator(_configuration, ConfigureLoggingWithoutConsole);
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

        var scrapAppService = serviceResolver.Get<IResourcesApplicationService>();
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

    [Verb(Description = "Downloads resources as given by the console input", Aliases = "d")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public async Task Download(
        [Description("Job definition name")]string? name = null,
        [Description("URL where the scrapping starts")]string? rootUrl = null,
        [Description("Download resources even if they are already downloaded")]bool downloadAlways = false,
        [Description("Pipeline")]string[]? pipeline = null)
    {
        var serviceResolver = new ServicesLocator(_configuration, ConfigureLoggingWithoutConsole);
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

        var scrapAppService = serviceResolver.Get<IDownloadApplicationService>();
        IEnumerable<string> inputLines = pipeline ?? ConsoleInput();
        foreach (var line in inputLines)
        {
            var split = line.Split(" ");
            var pageIndex = int.Parse(split[0]);
            var pageUrl = new Uri(split[1]);
            var resourceIndex = int.Parse(split[2]);
            var resourceUrl = new Uri(split[3]);
            await scrapAppService.DownloadAsync(newJob, pageUrl, pageIndex, resourceUrl, resourceIndex);
            Console.WriteLine($"Downloaded {resourceUrl}");
        }
    }

    [Verb(Description = "Lists all the pages reachable with the adjacency path", Aliases = "m")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public async Task MarkVisited(
        [Description("Job definition name")]string? name = null,
        [Description("URL where the scrapping starts")]string? rootUrl = null,
        [Description("Pipeline")]string[]? pipeline = null)
    {
        var serviceResolver = new ServicesLocator(_configuration, ConfigureLoggingWithoutConsole);
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

        var scrapAppService = serviceResolver.Get<IMarkVisitedApplicationService>();
        IEnumerable<string> inputLines = pipeline ?? ConsoleInput();
        foreach (var line in inputLines)
        {
            var pageUrl = new Uri(line);
            await scrapAppService.MarkVisitedPageAsync(newJob, pageUrl);
            Console.WriteLine($"Visited {pageUrl}");
        }
    }

    [Verb(Description = "Lists all the pages reachable with the adjacency path", Aliases = "db")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public async Task DatabaseSearch(
        [Description("Search with Regular Expression")]string? search = null,
        [Description("Delete results")]bool delete = false)
    {
        var serviceResolver = new ServicesLocator(_configuration, ConfigureLoggingWithoutConsole);

        var dbAppService = serviceResolver.Get<IDatabaseApplicationService>();
        search ??= ConsoleInput().First();
        if (delete)
        {
            await dbAppService.DeleteAsync(search);
            return;
        }

        var result = await dbAppService.SearchAsync(search);
        foreach (var line in result)
        {
            Console.WriteLine(line.Uri);
        }
    }

    [Verb(Description = "Configures the tool", Aliases = "c,config")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public void Configure(
        string? key = null,
        string? value = null)
    {
        var isInteractive = key == null;
        if (isInteractive)
        {
            PrintHeader();
        }

        var globalUserConfigFolder = GetGlobalUserConfigFolder();
        var globalUserConfigPath = Path.Combine(globalUserConfigFolder, "scrap-user.json");
        
        Directory.CreateDirectory(globalUserConfigFolder);
        if (File.Exists(globalUserConfigPath))
        {
            Console.WriteLine($"Global config file found at: {globalUserConfigPath}");
        }
        else
        {
            if (isInteractive)
            {
                Console.WriteLine(
                    $"Global config file not found. We are going to create a global config file and ask some values. This file is located at: {globalUserConfigPath}");
                Console.WriteLine(
                    "The global config file will not be modified or deleted by any install, update or uninstall of this tool.");
            }

            File.WriteAllText(globalUserConfigPath, "{ \"Scrap\": {}}");
            Console.WriteLine($"Created global config at: {globalUserConfigPath}");
        }

        var cfg =
            new ConfigurationBuilder()
                .AddJsonFile(globalUserConfigPath, optional: false, reloadOnChange: false)
                .Build();

        if (isInteractive)
        {
            SetUpGlobalConfigValuesInteractively(globalUserConfigFolder, globalUserConfigPath, cfg);
            return;
        }

        if (value == null)
        {
            Console.Error.WriteLine("You must set a value");
            return;
        }

        System.Diagnostics.Debug.Assert(key != null, $"{nameof(key)} != null");
        SetUpGlobalConfigValue(globalUserConfigFolder, globalUserConfigPath, key, value);
    }

    [Verb(Description = "Show version", Aliases = "v")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Version()
    {
        Console.WriteLine(GetVersion());
    }

    [PostVerbExecution]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public void After(PostVerbExecutionContext context)
    {
        if (_debug)
        {
            Console.ReadKey();
        }

        if (context.Failed)
        {
            Console.WriteLine($"ERROR: {context.Exception.Message}");
        }
    }

    private string GetGlobalUserConfigFolder()
        => _configuration[ConfigFolderEnvironment]
           ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".scrap");

    private record GlobalConfig(
        string Key,
        string DefaultValue,
        string Prompt);

    private void EnsureGlobalConfiguration(string globalUserConfigFolder, string globalUserConfigPath)
    {
        if (!File.Exists(globalUserConfigPath))
        {
            throw new ScrapException("The tool is not properly configured; call 'scrap config'.");
        }

        if (GetGlobalConfigs(globalUserConfigFolder).Any(config => _configuration[config.Key] == null))
        {
            throw new ScrapException("The tool is not properly configured; call 'scrap config'.");
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

    private static void SetUpGlobalConfigValuesInteractively(
        string globalUserConfigFolder,
        string globalUserConfigPath,
        IConfiguration cfg)
    {
        CreateGlobalConfigFile(globalUserConfigFolder, globalUserConfigPath);

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
            
        KeyValuePair<string, object?>? EnsureGlobalConfigValue(GlobalConfig globalConfig)
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

            return new KeyValuePair<string, object?>(key, value);
        }
    }

    private static void CreateGlobalConfigFile(string globalUserConfigFolder, string globalUserConfigPath)
    {
        Directory.CreateDirectory(globalUserConfigFolder);
        if (!File.Exists(globalUserConfigPath))
        {
            Console.WriteLine(
                "Global config file not found. We are going to create a global config file and ask some values. This file is located at: {globalUserConfigPath}");
            Console.WriteLine(
                $"The global config file will not be modified or deleted by any install, update or uninstall of this tool.");
            File.WriteAllText(globalUserConfigPath, "{ \"Scrap\": {}}");
            Console.WriteLine($"Created global config at: {globalUserConfigPath}");
        }
    }

    private static void SetUpGlobalConfigValue(
        string globalUserConfigFolder,
        string globalUserConfigPath,
        string key,
        string value)
    {
        CreateGlobalConfigFile(globalUserConfigFolder, globalUserConfigPath);

        var update = GetGlobalConfigs(globalUserConfigFolder).SingleOrDefault(x => x.Key == key);
        if (update == null)
        {
            Console.Error.WriteLine("Key not found!");
        }
        
        var updater = new JsonUpdater(globalUserConfigPath);
        updater.AddOrUpdate(new[] { new KeyValuePair<string, object?>(key, value) });
        Console.WriteLine($"{key}={value}");
    }

    private void ConfigureLoggingWithConsole(ILoggingBuilder builder) => ConfigureLogging(builder, true);
    private void ConfigureLoggingWithoutConsole(ILoggingBuilder builder) => ConfigureLogging(builder, false);
    private void ConfigureLogging(ILoggingBuilder builder, bool withConsole)
    {
        builder.ClearProviders();
        builder.AddConfiguration(_configuration.GetSection("Logging"));
        builder.AddFile(_configuration.GetSection("Logging:File"), options => options.FolderPath = GetGlobalUserConfigFolder());

        if (!_verbose)
        {
            builder.AddFilter(level => level != LogLevel.Trace);
        }

        if (withConsole)
        {
            builder.AddConsole();
            builder.AddConsoleFormatter<CustomConsoleFormatter, CustomConsoleFormatterOptions>();
        }
    }

    private static void PrintHeader()
    {
        var version = GetVersion();
        var currentColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(FiggleFonts.Doom.Render($"scrap {version}"));
        Console.WriteLine("Command line tool for generic web scrapping");
        Console.WriteLine();
        Console.ForegroundColor = currentColor;
    }

    private static string? GetVersion() =>
        Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

    private static IEnumerable<string> ConsoleInput()
    {
        while (Console.ReadLine() is { } line)
        {
            yield return line;
        }
    }

    private async Task<JobDto?> BuildJobDto(
        ServicesLocator serviceLocator,
        string? name,
        string? rootUrl,
        bool? fullScan,
        bool? downloadAlways,
        bool? disableMarkingVisited,
        bool? disableResourceWrites)
    {
        var definitionsApplicationService = serviceLocator.Get<JobDefinitionsApplicationService>();
        var logger = serviceLocator.Get<ILogger<ScrapCommandLine>>();
        var envName = _configuration[JobDefNameEnvironment];
        var envRootUrl = _configuration[JobDefRootUrlEnvironment];
        
        var jobDef = await GetJobDefinition(name, rootUrl, definitionsApplicationService, envName, envRootUrl, logger);

        if (jobDef == null)
        {
            return null;
        }
        
        logger.LogInformation("The following job def will be run: {JobDef}", jobDef);
        
        return new JobDto(jobDef, rootUrl ?? envRootUrl, fullScan, null, downloadAlways, disableMarkingVisited, disableResourceWrites);        
    }

    private async Task<JobDefinitionDto?> GetJobDefinition(
        string? name,
        string? rootUrl,
        JobDefinitionsApplicationService definitionsApplicationService,
        string? envName,
        string? envRootUrl,
        ILogger logger)
    {
        JobDefinitionDto? jobDef = null;
        if (name != null)
        {
            jobDef = await definitionsApplicationService.FindJobByNameAsync(name);
            if (jobDef == null)
            {
                logger.LogError("Job definition {Name} does not exist", name);
            }

            return jobDef;
        }
        
        if (rootUrl != null)
        {
            var jobDefs = await definitionsApplicationService.FindJobsByRootUrlAsync(rootUrl).ToArrayAsync();
            if (jobDefs.Length == 0)
            {
                logger.LogWarning("No job definition matches with {RootUrl}", rootUrl);
            }
            else if (jobDefs.Length > 1)
            {
                logger.LogWarning("More than one definition matched with {RootUrl}", rootUrl);
            }
            else
            {
                return jobDefs[0];
            }
        }
        
        if (envName != null)
        {
            jobDef = await definitionsApplicationService.FindJobByNameAsync(envName);
            if (jobDef == null)
            {
                logger.LogError("Job definition {Name} does not exist", envName);
            }

            return jobDef;
        }
        
        if (envRootUrl != null)
        {
            var jobDefs = await definitionsApplicationService.FindJobsByRootUrlAsync(envRootUrl).ToArrayAsync();
            if (jobDefs.Length == 0)
            {
                logger.LogWarning("No job definition matches with {RootUrl}", envRootUrl);
            }
            else if (jobDefs.Length > 1)
            {
                logger.LogWarning("More than one definition matched with {RootUrl}", envRootUrl);
            }
            else
            {
                return jobDefs[0];
            }
        }

        if (jobDef == null)
        {
            logger.LogWarning("No single job definition was found, nothing will be done");
        }

        return jobDef;
    }

    private static void Help(string helpText)
    {
        Console.WriteLine("SCRAP is a tool for generic web scrapping. To set it up, head to the project docs: https://github.com/icalvo/scrap");
        Console.WriteLine(helpText);
    }

    private static void Exception(ExceptionContext c, IEnumerable<string> args)
    {
        if (c.Exception is TargetInvocationException { InnerException: ScrapException s})
        {
            Console.Error.WriteLine("{0}", s.Message);
            return;
        }

        Console.Error.WriteLine("Parsing error: {0}", c.Exception.Demystify());
        Console.Error.WriteLine("Arguments:");
        foreach (var (arg, idx) in args.Select((arg, idx) => (arg, idx)))
        {
            Console.WriteLine("Arg {0}: {1}", idx, arg);
        }
    }

    private class ScrapException : Exception
    {
        public ScrapException(string message) : base(message)
        {
        }
    }
}
