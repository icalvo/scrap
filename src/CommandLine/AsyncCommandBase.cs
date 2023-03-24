﻿using System.Diagnostics;
using Figgle;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scrap.Application;
using Scrap.Domain;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Jobs;
using Scrap.Domain.Resources.FileSystem;
using Scrap.Infrastructure;
using Spectre.Console.Cli;

namespace Scrap.CommandLine;

internal abstract class AsyncCommandBase<TSettings> : AsyncCommand<TSettings> where TSettings : SettingsBase
{
    private bool _verbose;

    protected readonly IConfiguration Configuration;
    protected readonly IFileSystem FileSystem;
    private readonly IOAuthCodeGetter _oAuthCodeGetter;

    protected AsyncCommandBase(IConfiguration configuration, IOAuthCodeGetter oAuthCodeGetter, IFileSystem fileSystem)
    {
        Configuration = configuration;
        _oAuthCodeGetter = oAuthCodeGetter;
        FileSystem = fileSystem;
    }
    
    public override async Task<int> ExecuteAsync(CommandContext context, TSettings settings)
    {
        if (settings.Debug)
        {
            Debugger.Launch();
        }

        _verbose = settings.Verbose;

        if (context.Name != ConfigureCommand.Name)
        {
            await EnsureGlobalConfigurationAsync();
        }
        
        var result = await CommandExecuteAsync(settings);
        
        if (settings.Debug)
        {
            Console.ReadKey();
        }

        return result;
    }

    private async Task EnsureGlobalConfigurationAsync()
    {
        var globalUserConfigPath = Configuration.GlobalUserConfigPath() ?? FileSystem.DefaultGlobalUserConfigFile;
        if (!await FileSystem.File.ExistsAsync(globalUserConfigPath))
        {
            Console.WriteLine($"The global config file [{globalUserConfigPath}] does not exist");
            throw new ScrapException("The tool is not properly configured; call 'scrap config'");
        }

        var globalUserConfigFolder = FileSystem.Path.GetDirectoryName(globalUserConfigPath);
        var unsetKeys = GetGlobalConfigs(globalUserConfigFolder)
            .Where(config => !config.Optional && Configuration[config.Key] == null).ToArray();
        if (!unsetKeys.Any())
        {
            return;
        }

        var keyList = string.Join(", ", unsetKeys.Select(x => x.Key));
        Console.WriteLine($"Unset configuration keys: {keyList}");
        throw new ScrapException("The tool is not properly configured; call 'scrap config'");
    }

    protected abstract Task<int> CommandExecuteAsync(TSettings settings);

    protected static IEnumerable<GlobalConfig> GetGlobalConfigs(string globalUserConfigFolder) =>
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

    protected static void PrintHeader()
    {
        var version = VersionCommand.GetVersion();
        var currentColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(FiggleFonts.Doom.Render($"scrap {version}"));
        Console.WriteLine("Command line tool for generic web scrapping");
        Console.WriteLine();
        Console.ForegroundColor = currentColor;
    }

    protected ServiceProvider BuildServiceProviderWithConsole() => BuildServiceProvider(true);
    protected ServiceProvider BuildServiceProviderWithoutConsole() => BuildServiceProvider(false);
    private ServiceProvider BuildServiceProvider(bool withConsole)
    {
        Debug.Assert(Configuration != null, nameof(Configuration) + " != null");
        return new ServiceCollection()
            .ConfigureDomainServices()
            .ConfigureApplicationServices()
            .ConfigureInfrastructureServices(Configuration, withConsole, _verbose, _oAuthCodeGetter)
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
    }

    protected static async Task<JobDefinitionDto?> GetJobDefinitionAsync(
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
            jobDef = await definitionsApplicationService.FindByNameAsync(name);
            if (jobDef == null)
            {
                logger.LogError("Job definition {Name} does not exist", name);
            }

            return jobDef;
        }

        if (rootUrl != null)
        {
            var jobDefs = await definitionsApplicationService.FindByRootUrlAsync(rootUrl).ToArrayAsync();
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
            jobDef = await definitionsApplicationService.FindByNameAsync(envName);
            if (jobDef == null)
            {
                logger.LogError("Job definition {Name} does not exist", envName);
            }

            return jobDef;
        }

        if (envRootUrl != null)
        {
            var jobDefs = await definitionsApplicationService.FindByRootUrlAsync(envRootUrl).ToArrayAsync();
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

    protected async Task<JobDto?> BuildJobDtoAsync(
        IServiceProvider serviceLocator,
        string? name,
        string? rootUrl,
        bool? fullScan,
        bool? downloadAlways,
        bool? disableMarkingVisited,
        bool? disableResourceWrites)
    {
        var definitionsApplicationService = serviceLocator.GetRequiredService<JobDefinitionsApplicationService>();
        var logger = serviceLocator.GetRequiredService<ILogger<AsyncCommandBase<TSettings>>>();
        Debug.Assert(Configuration != null, nameof(Configuration) + " != null");
        var envName = Configuration.JobDefName();
        var envRootUrl = Configuration.JobDefRootUrl();

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

    protected static IEnumerable<string> ConsoleInput()
    {
        while (Console.ReadLine() is { } line)
        {
            yield return line;
        }
    }
}