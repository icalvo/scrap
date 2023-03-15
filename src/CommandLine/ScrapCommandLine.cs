using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CLAP;
using CLAP.Interception;
using Figgle;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scrap.Application;
using Scrap.Application.Scrap;
using Scrap.CommandLine.Logging;
using Scrap.Common;
using Scrap.DependencyInjection;
using Scrap.DependencyInjection.Factories;
using Scrap.Domain;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Jobs;
using Scrap.Domain.Resources.FileSystem;

namespace Scrap.CommandLine;

public class ScrapCommandLine
{
    private const string JobDefNameEnvironment = "JobDefinition:DefaultName";
    private const string JobDefRootUrlEnvironment = "JobDefinition:DefaultRootUrl";
    private const string ConfigFolderEnvironment = "GlobalConfigurationFolder";

    private static readonly string DefaultGlobalUserConfigFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".scrap");

    private static readonly IOAuthCodeGetter OAuthCodeGetter = new ConsoleOAuthCodeGetter();
    private readonly IConfiguration _configuration;
    private readonly string _globalUserConfigFolder;
    private bool _debug;
    private bool _verbose;

    public ScrapCommandLine(Parser<ScrapCommandLine> parser, string[] args)
    {
        parser.Register.HelpHandler("help,h,?", HelpHandler);
        parser.Register.ErrorHandler((Action<ExceptionContext>)(c => ErrorHandler(c, args)));
        if (_debug)
        {
            Debugger.Launch();
        }

        const string environmentVarPrefix = "Scrap_";
        _configuration = new ConfigurationBuilder().AddEnvironmentVariables(environmentVarPrefix).Build();
        _globalUserConfigFolder = GetGlobalUserConfigFolder();
        if (_globalUserConfigFolder == DefaultGlobalUserConfigFolder)
        {
            if (!Directory.Exists(DefaultGlobalUserConfigFolder))
            {
                Directory.CreateDirectory(DefaultGlobalUserConfigFolder);
            }
        }

        var globalUserConfigPath = Path.Combine(_globalUserConfigFolder, "scrap-user.json");
        var configBuilder = new ConfigurationBuilder().AddJsonFile("scrap.json", false, false);
        if (File.Exists(globalUserConfigPath))
        {
            _ = configBuilder.AddJsonFile(globalUserConfigPath, false, false);
        }

        configBuilder.AddJsonFile("scrap.Development.json", optional: true, reloadOnChange: false);
        configBuilder.AddEnvironmentVariables(environmentVarPrefix);
        _configuration = configBuilder.Build();
    }

    [PreVerbExecution]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private void Before(PreVerbExecutionContext context)
    {
        if (!context.Method.Names.Contains("configure"))
        {
            EnsureGlobalConfiguration(_globalUserConfigFolder);
        }
    }

    [Global(Aliases = "dbg", Description = "Runs a debugger session at the beginning")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public void Debug() => _debug = true;

    [Global(Aliases = "v", Description = "Verbose output")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public void Verbose() => _verbose = true;

    [Verb(IsDefault = true, Description = "Executes a job definition")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public async Task Scrap(
        [Description("Job definition name")] [Aliases("n")] string? name = null,
        [Description("URLs where the scrapping starts")] [Aliases("r")] string? rootUrl = null,
        [Description("Navigate through already visited pages")] [Aliases("f")] bool fullScan = false,
        [Description("Download resources even if they are already downloaded")] [Aliases("d")] bool downloadAlways =
            false,
        [Description("Disable mark as visited")] [Aliases("dmv")] bool disableMarkingVisited = false,
        [Description("Disable writing the resource")] [Aliases("dwr")] bool disableResourceWrites = false)
    {
        PrintHeader();

        var serviceResolver = ServicesLocator.Build(_configuration, ConfigureLoggingWithConsole, OAuthCodeGetter);
        var logger = serviceResolver.GetRequiredService<ILogger<ScrapCommandLine>>();
        var definitionsApplicationService = serviceResolver.GetRequiredService<JobDefinitionsApplicationService>();

        var envRootUrl = _configuration[JobDefRootUrlEnvironment];
        var envName = _configuration[JobDefNameEnvironment];

        var jobDef = await GetJobDefinitionAsync(
            name,
            rootUrl,
            definitionsApplicationService,
            envName,
            envRootUrl,
            logger);

        var jobDefs = jobDef == null ? Array.Empty<JobDefinitionDto>() : new[] { jobDef };

        await ScrapMultipleJobDefsAsync(
            fullScan,
            downloadAlways,
            disableMarkingVisited,
            disableResourceWrites,
            logger,
            name != null,
            jobDefs,
            rootUrl,
            envRootUrl,
            serviceResolver);
    }


    [Verb(Description = "Executes all default job definitions", Aliases = "a")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public async Task All(
        [Description("Navigate through already visited pages")] [Aliases("f")] bool fullScan = false,
        [Description("Download resources even if they are already downloaded")] [Aliases("d")] bool downloadAlways =
            false,
        [Description("Disable mark as visited")] [Aliases("dmv")] bool disableMarkingVisited = false,
        [Description("Disable writing the resource")] [Aliases("dwr")] bool disableResourceWrites = false)
    {
        PrintHeader();

        var serviceResolver = ServicesLocator.Build(_configuration, ConfigureLoggingWithConsole, OAuthCodeGetter);
        var logger = serviceResolver.GetRequiredService<ILogger<ScrapCommandLine>>();
        var definitionsApplicationService = serviceResolver.GetRequiredService<JobDefinitionsApplicationService>();

        var jobDefs = await definitionsApplicationService.GetJobsAsync()
            .Where(x => x.RootUrl != null && x.HasResourceCapabilities()).ToListAsync();

        await ScrapMultipleJobDefsAsync(
            fullScan,
            downloadAlways,
            disableMarkingVisited,
            disableResourceWrites,
            logger,
            true,
            jobDefs,
            null,
            null,
            serviceResolver);
    }

    private static async Task ScrapMultipleJobDefsAsync(
        bool fullScan,
        bool downloadAlways,
        bool disableMarkingVisited,
        bool disableResourceWrites,
        ILogger logger,
        bool showJobDefs,
        IEnumerable<JobDefinitionDto> jobDefs,
        string? rootUrl,
        string? envRootUrl,
        IServiceProvider serviceResolver)
    {
        var jobDefsArray = jobDefs as JobDefinitionDto[] ?? jobDefs.ToArray();
        if (!jobDefsArray.Any())
        {
            logger.LogWarning("No job definition found, nothing will be done");
            return;
        }

        if (showJobDefs)
        {
            logger.LogInformation(
                "The following job def(s). will be run: {JobDefs}",
                string.Join(", ", jobDefsArray.Select(x => x.Name)));
        }

        foreach (var jobDef in jobDefsArray)
        {
            var newJob = new JobDto(
                jobDef,
                rootUrl ?? envRootUrl,
                fullScan,
                null,
                downloadAlways,
                disableMarkingVisited,
                disableResourceWrites);
            var scrapAppService = serviceResolver.GetRequiredService<ScrapApplicationService>();
            logger.LogInformation("Starting {Definition}...", jobDef.Name);
            await scrapAppService.ScrapAsync(newJob);
            logger.LogInformation("Finished!");
        }
    }

    [Verb(Description = "Lists all the pages reachable with the adjacency path", Aliases = "t")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public async Task Traverse(
        [Description("Job definition name")] string? name = null,
        [Description("URL where the scrapping starts")] string? rootUrl = null,
        [Description("Navigate through already visited pages")] bool fullScan = false)
    {
        var serviceResolver = ServicesLocator.Build(_configuration, ConfigureLoggingWithoutConsole, OAuthCodeGetter);
        var newJob = await BuildJobDtoAsync(serviceResolver, name, rootUrl, fullScan, false, true, true);
        if (newJob == null)
        {
            return;
        }

        var service = serviceResolver.GetRequiredService<ITraversalApplicationService>();
        await service.TraverseAsync(newJob).ForEachAsync(x => Console.WriteLine(x));
    }

    [Verb(Description = "Lists all the resources available in pages provided by console input", Aliases = "r")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public async Task Resources(
        [Description("Job definition name")] string? name = null,
        [Description("URL where the scrapping starts")] string? rootUrl = null,
        [Description("Page URLs [pipeline]")] string[]? pageUrls = null,
        [Description("Output only the resource link instead of the format expected by 'scrap download'")]
        bool onlyResourceLink = false)
    {
        var serviceResolver = ServicesLocator.Build(_configuration, ConfigureLoggingWithoutConsole, OAuthCodeGetter);
        var newJob = await BuildJobDtoAsync(serviceResolver, name, rootUrl, false, false, true, true);
        if (newJob == null)
        {
            return;
        }

        var scrapAppService = serviceResolver.GetRequiredService<IResourcesApplicationService>();
        var pageIndex = 0;
        var inputLines = pageUrls ?? ConsoleInput();
        foreach (var line in inputLines)
        {
            var pageUrl = new Uri(line);
            await scrapAppService.GetResourcesAsync(newJob, pageUrl, pageIndex).ForEachAsync(
                (resourceUrl, resourceIndex) =>
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
        [Description("Job definition name")] string? name = null,
        [Description("URL where the scrapping starts")] string? rootUrl = null,
        [Description("Download resources even if they are already downloaded")] bool downloadAlways = false,
        [Description("Resource URLs to download [pipeline]")] string[]? resourceUrls = null)
    {
        var serviceResolver = ServicesLocator.Build(_configuration, ConfigureLoggingWithoutConsole, OAuthCodeGetter);
        var newJob = await BuildJobDtoAsync(serviceResolver, name, rootUrl, false, downloadAlways, true, false);
        if (newJob == null)
        {
            return;
        }

        var scrapAppService = serviceResolver.GetRequiredService<IDownloadApplicationService>();
        var inputLines = resourceUrls ?? ConsoleInput();
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

    [Verb(Description = "Adds a visited page", Aliases = "m,mv")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public async Task MarkVisited([Description("URL [pipeline]")] string[]? url = null)
    {
        var serviceResolver = ServicesLocator.Build(_configuration, ConfigureLoggingWithoutConsole, OAuthCodeGetter);

        var visitedPagesAppService = serviceResolver.GetRequiredService<IVisitedPagesApplicationService>();
        var inputLines = url ?? ConsoleInput();
        foreach (var line in inputLines)
        {
            var pageUrl = new Uri(line);
            await visitedPagesAppService.MarkVisitedPageAsync(pageUrl);
            Console.WriteLine($"Visited {pageUrl}");
        }
    }

    [Verb(Description = "Searches visited pages", Aliases = "sv")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public async Task SearchVisited([Description("Search with Regular Expression [pipeline]")] string? search = null)
    {
        var serviceResolver = ServicesLocator.Build(_configuration, ConfigureLoggingWithoutConsole, OAuthCodeGetter);

        var visitedPagesAppService = serviceResolver.GetRequiredService<IVisitedPagesApplicationService>();
        search ??= ConsoleInput().First();
        var result = await visitedPagesAppService.SearchAsync(search);
        foreach (var line in result)
        {
            Console.WriteLine(line.Uri);
        }
    }

    [Verb(Description = "Searches and removes visited pages", Aliases = "dv")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public async Task DeleteVisited([Description("Search with Regular Expression [pipeline]")] string? search = null)
    {
        var serviceResolver = ServicesLocator.Build(_configuration, ConfigureLoggingWithoutConsole, OAuthCodeGetter);

        var visitedPagesAppService = serviceResolver.GetRequiredService<IVisitedPagesApplicationService>();
        search ??= ConsoleInput().First();
        var result = await visitedPagesAppService.SearchAsync(search);
        foreach (var line in result)
        {
            Console.WriteLine(line.Uri);
        }

        Console.WriteLine();
        Console.WriteLine("Deleting...");
        await visitedPagesAppService.DeleteAsync(search);
        Console.WriteLine("Finished!");
    }

    [Verb(Description = "Configures the tool", Aliases = "c,config")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public async Task Configure(
        [Description($"Config key (e.g. {ConfigKeys.BaseRootFolder})")] string? key = null,
        [Description("Value for the key")] string? value = null)
    {
        if (key == null)
        {
            await ConfigureInteractiveAsync();
        }
        else
        {
            await ConfigureNonInteractiveAsync(key, value);
        }
    }

    private async Task ConfigureInteractiveAsync()
    {
        PrintHeader();

        var globalUserConfigFolder = GetGlobalUserConfigFolder();
        var globalUserConfigPath = Path.Combine(globalUserConfigFolder, "scrap-user.json");

        Directory.CreateDirectory(globalUserConfigFolder);
        if (File.Exists(globalUserConfigPath))
        {
            Console.WriteLine($"Global config file found at: {globalUserConfigPath}");
        }
        else
        {
            Console.WriteLine(
                $"Global config file not found. We are going to create a global config file and ask some values. This file is located at: {globalUserConfigPath}");
            Console.WriteLine(
                "The global config file will not be modified or deleted by any install, update or uninstall of this tool.");

            await File.WriteAllTextAsync(globalUserConfigPath, "{ \"Scrap\": {}}");
            Console.WriteLine($"Created global config at: {globalUserConfigPath}");
        }

        var cfg = new ConfigurationBuilder().AddJsonFile(globalUserConfigPath, false, false).Build();

        CreateGlobalConfigFile(globalUserConfigFolder, globalUserConfigPath);

        var updates = GetGlobalConfigs(globalUserConfigFolder).Select(AskGlobalConfigValue).RemoveNulls().ToArray();
        if (updates.Length == 0)
        {
            Console.WriteLine("Nothing changed!");
        }
        else
        {
            Console.WriteLine($"Adding or updating {updates.Length} config value(s)");
            var updater = new JsonUpdater(new LocalFileSystem(), globalUserConfigPath);
            await updater.AddOrUpdateAsync(updates);
        }

        KeyValuePair<string, object?>? AskGlobalConfigValue(GlobalConfig globalConfig)
        {
            var (key, defaultValue, prompt, _) = globalConfig;
            var promptDefaultValue = cfg[key] ?? defaultValue;

            Console.Write($"{prompt} [{promptDefaultValue}]: ");
            var value = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(value))
            {
                value = promptDefaultValue;
            }

            if (value == cfg[key])
            {
                return null;
            }

            return new KeyValuePair<string, object?>(key, value);
        }
    }

    private async Task ConfigureNonInteractiveAsync(string key, string? value = null)
    {
        var globalUserConfigFolder = GetGlobalUserConfigFolder();
        var globalUserConfigPath = Path.Combine(globalUserConfigFolder, "scrap-user.json");

        Directory.CreateDirectory(globalUserConfigFolder);
        if (File.Exists(globalUserConfigPath))
        {
            Console.WriteLine($"Global config file found at: {globalUserConfigPath}");
        }
        else
        {
            await File.WriteAllTextAsync(globalUserConfigPath, "{ \"Scrap\": {}}");
            Console.WriteLine($"Created global config at: {globalUserConfigPath}");
        }

        if (value == null)
        {
            await Console.Error.WriteLineAsync("You must set a value");
            return;
        }

        System.Diagnostics.Debug.Assert(key != null, $"{nameof(key)} != null");
        CreateGlobalConfigFile(globalUserConfigFolder, globalUserConfigPath);

        var update = GetGlobalConfigs(globalUserConfigFolder).SingleOrDefault(x => x.Key == key);
        if (update == null)
        {
            await Console.Error.WriteLineAsync("Key not found!");
        }

        var updater = new JsonUpdater(new LocalFileSystem(), globalUserConfigPath);
        await updater.AddOrUpdateAsync(new[] { new KeyValuePair<string, object?>(key, value) });
        Console.WriteLine($"{key}={value}");
    }

    [Verb(Description = "Show configuration", Aliases = "sc")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public void ShowConfig()
    {
        var root = (IConfigurationRoot)_configuration;
        Console.WriteLine(root.GetDebugView());
    }

    [Verb(Description = "Show version", Aliases = "v")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Version() => Console.WriteLine(GetVersion());

    [PostVerbExecution]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public void After(PostVerbExecutionContext _)
    {
        if (_debug)
        {
            Console.ReadKey();
        }
    }

    private string GetGlobalUserConfigFolder() =>
        _configuration[ConfigFolderEnvironment] ?? DefaultGlobalUserConfigFolder;

    private void EnsureGlobalConfiguration(string globalUserConfigFolder)
    {
        if (!Directory.Exists(globalUserConfigFolder))
        {
            Console.WriteLine($"The global config folder [{globalUserConfigFolder}] does not exist");
            throw new ScrapException("The tool is not properly configured; call 'scrap config'");
        }

        var unsetKeys = GetGlobalConfigs(globalUserConfigFolder)
            .Where(config => !config.Optional && _configuration[config.Key] == null).ToArray();
        if (!unsetKeys.Any())
        {
            return;
        }

        var keyList = string.Join(", ", unsetKeys.Select(x => x.Key));
        Console.WriteLine($"Unset configuration keys: {keyList}");
        throw new ScrapException("The tool is not properly configured; call 'scrap config'");
    }

    private static IEnumerable<GlobalConfig> GetGlobalConfigs(string globalUserConfigFolder) =>
        new[]
        {
            new GlobalConfig(
                ConfigKeys.Definitions,
                Path.Combine(globalUserConfigFolder, "jobDefinitions.json"),
                "Path for job definitions JSON"),
            new GlobalConfig(
                ConfigKeys.Database,
                $"Filename={Path.Combine(globalUserConfigFolder, "scrap.db")};Connection=shared",
                "Connection string for visited page database"),
            new GlobalConfig(
                ConfigKeys.FileSystemType,
                "local",
                "Filesystem type (local/dropbox)",
                Optional: true),
            new GlobalConfig(
                ConfigKeys.BaseRootFolder,
                null,
                "Base download path for your file-based resource repository",
                Optional: true)
        };

    private static void CreateGlobalConfigFile(string globalUserConfigFolder, string globalUserConfigPath)
    {
        Directory.CreateDirectory(globalUserConfigFolder);
        if (!File.Exists(globalUserConfigPath))
        {
            Console.WriteLine(
                "Global config file not found. We are going to create a global config file and ask some values. This file is located at: {globalUserConfigPath}");
            Console.WriteLine(
                "The global config file will not be modified or deleted by any install, update or uninstall of this tool.");
            File.WriteAllText(globalUserConfigPath, "{ \"Scrap\": {}}");
            Console.WriteLine($"Created global config at: {globalUserConfigPath}");
        }
    }

    private void ConfigureLoggingWithConsole(ILoggingBuilder builder) => ConfigureLogging(builder, true);

    private void ConfigureLoggingWithoutConsole(ILoggingBuilder builder) => ConfigureLogging(builder, false);

    private void ConfigureLogging(ILoggingBuilder builder, bool withConsole)
    {
        builder.ClearProviders();
        builder.AddConfiguration(_configuration.GetSection("Logging"));
        var globalUserConfigFolder = GetGlobalUserConfigFolder();
        if (Directory.Exists(globalUserConfigFolder))
        {
            builder.AddFile(
                _configuration.GetSection("Logging:File"),
                options => options.FolderPath = globalUserConfigFolder);
        }

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
        Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

    private static IEnumerable<string> ConsoleInput()
    {
        while (Console.ReadLine() is { } line)
        {
            yield return line;
        }
    }

    private async Task<JobDto?> BuildJobDtoAsync(
        IServiceProvider serviceLocator,
        string? name,
        string? rootUrl,
        bool? fullScan,
        bool? downloadAlways,
        bool? disableMarkingVisited,
        bool? disableResourceWrites)
    {
        var definitionsApplicationService = serviceLocator.GetRequiredService<JobDefinitionsApplicationService>();
        var logger = serviceLocator.GetRequiredService<ILogger<ScrapCommandLine>>();
        var envName = _configuration[JobDefNameEnvironment];
        var envRootUrl = _configuration[JobDefRootUrlEnvironment];

        var jobDef = await GetJobDefinitionAsync(
            name,
            rootUrl,
            definitionsApplicationService,
            envName,
            envRootUrl,
            logger);

        if (jobDef == null)
        {
            return null;
        }

        logger.LogInformation("The following job def will be run: {JobDef}", jobDef);

        return new JobDto(
            jobDef,
            rootUrl ?? envRootUrl,
            fullScan,
            null,
            downloadAlways,
            disableMarkingVisited,
            disableResourceWrites);
    }

    private static async Task<JobDefinitionDto?> GetJobDefinitionAsync(
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

    private static void HelpHandler(string helpText)
    {
        Console.WriteLine(
            "SCRAP is a tool for generic web scrapping. To set it up, head to the project docs: https://github.com/icalvo/scrap");
        Console.WriteLine(
            "[pipeline] in the description of a parameter means that if that parameter is not provided, it will be taken from the shell pipeline or the standard input.");
        Console.WriteLine(helpText);
    }

    private void ErrorHandler(ExceptionContext c, IEnumerable<string> args)
    {
        var ex = c.Exception;
        if (c.Exception is TargetInvocationException && c.Exception.InnerException != null)
        {
            ex = c.Exception.InnerException;
        }

        var serviceResolver = ServicesLocator.Build(_configuration, ConfigureLoggingWithConsole, OAuthCodeGetter);
        var logger = serviceResolver.GetRequiredService<ILogger<ScrapCommandLine>>();

        if (ex is ScrapException)
        {
            Console.WriteLine(ex.Message);
            return;
        }

        logger.LogError("ERROR: {Message}", ex.Message);
        logger.LogTrace("{Stacktrace}", ex.Demystify().StackTrace);
        logger.LogDebug("Arguments:");
        foreach (var (arg, idx) in args.Select((arg, idx) => (arg, idx)))
        {
            logger.LogDebug("Arg {Index}: {Argument}", idx, arg);
        }
    }

    private record GlobalConfig(string Key, string? DefaultValue, string Prompt, bool Optional = false);

    private class ScrapException : Exception
    {
        public ScrapException(string message) : base(message)
        {
        }
    }
}
