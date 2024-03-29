﻿using System.Diagnostics;
using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scrap.CommandLine;
using Scrap.CommandLine.Commands;
using Scrap.Domain;
using Scrap.Infrastructure;
using Scrap.Infrastructure.Factories;

TaskScheduler.UnobservedTaskException += (_, eventArgs) =>
    Console.Error.WriteLine($"Unobserved exception: {eventArgs.Exception}");

var (cfg, sc) = await BuildServiceCollection();
sc.AddSingleton<IGlobalConfigurationChecker, GlobalConfigurationChecker>();

var commandSetups = new ICommandSetup[]
{
    BuildCommandSetup<ScrapVerb, ScrapOneOptions>(),
    BuildCommandSetup<AllVerb, ScrapAllOptions>(),
    BuildCommandSetup<ConfigureVerb, ConfigureOptions>(),
    BuildCommandSetup<DeleteVisitedVerb, DeleteVisitedOptions>(),
    BuildCommandSetup<DownloadVerb, DownloadOptions>(),
    BuildCommandSetup<ResourcesVerb, ResourcesOptions>(),
    BuildCommandSetup<SearchVisitedVerb, SearchVisitedOptions>(),
    BuildCommandSetup<ShowConfigVerb, ShowConfigOptions>(),
    BuildCommandSetup<TraverseVerb, TraverseOptions>()
};

var optionTypes = commandSetups.Select(x => x.OptionsType).ToArray();
var parser = new Parser(settings => settings.HelpWriter = null);
var parserResult = parser.ParseArguments(args, optionTypes);
await parserResult.WithNotParsed(errors => DisplayHelp(parserResult)).WithParsedAsync(
    async options =>
    {
        try
        {
            if (options is OptionsBase { Debug: true })
            {
                Debugger.Launch();
            }

            var optionsType = options.GetType();
            var commandSetup = commandSetups.First(commandSetup => optionsType == commandSetup.OptionsType);
            await commandSetup.ExecuteAsync(options);
        }
        catch (Exception ex)
        {
            sc.ConfigureLogging(cfg, true, false);
            await using var sp = sc.BuildServiceProvider();
            var logger = sp.GetService<ILogger<Program>>();
            logger?.LogError("{ExceptionMessage}", ex.Message);
            if (options is OptionsBase { Verbose: false, ConsoleLog: true })
            {
                logger?.LogError(
                    "{Advice}",
                    $"To get more details please try again with verbose option (-{OptionsBase.VerboseLetter})");
            }
        }
    });

return;

static void DisplayHelp<T>(ParserResult<T> result)
{
    var helpText = HelpText.AutoBuild(
        result,
        h =>
        {
            h.MaximumDisplayWidth = Console.WindowWidth;
            h.AdditionalNewLineAfterOption = false;
            h.Heading = HeadingInfo.Default;
            h.Copyright = CopyrightInfo.Default;
            return h;
        });
    Console.WriteLine(helpText);
}

CommandSetup<TCommand, TOptions> BuildCommandSetup<TCommand, TOptions>()
    where TCommand : class, IVerb<TCommand, TOptions>
    where TOptions : OptionsBase =>
    new(cfg, sc);

async Task<(IConfiguration, IServiceCollection)> BuildServiceCollection()
{
    const string environmentVarPrefix = "Scrap_";
    IOAuthCodeGetter oAuthCodeGetter = new ConsoleOAuthCodeGetter();
    var envConfiguration = new ConfigurationBuilder().AddEnvironmentVariables(environmentVarPrefix).Build();

    var loggerFactory = new ServiceCollection().AddLogging(c => c.AddDebug()).BuildServiceProvider()
        .GetRequiredService<ILoggerFactory>();
    var log = loggerFactory.CreateLogger("Initialization");

    var fileSystemType = envConfiguration.FileSystemType()?.ToLowerInvariant() ?? "local";
    log.LogInformation("FILE SYSTEM: {FileSystemType}", fileSystemType);

    var fileSystemFactory = new FileSystemFactory(
        oAuthCodeGetter,
        fileSystemType,
        loggerFactory.CreateLogger<FileSystemFactory>());
    var fileSystem = await fileSystemFactory.BuildAsync(false);

    var globalUserConfigPath = envConfiguration.GlobalUserConfigPath();
    log.LogInformation("ENV GLOBAL CONFIG PATH: {GlobalUserConfigPath}", globalUserConfigPath);
    string globalUserConfigFolder;

    if (globalUserConfigPath == null)
    {
        globalUserConfigPath = fileSystem.DefaultGlobalUserConfigFile;
        globalUserConfigFolder = fileSystem.Path.GetDirectoryName(globalUserConfigPath);
        log.LogInformation("CREATING GLOBAL CONFIG PATH FOLDER: {GlobalUserConfigFolder}", globalUserConfigFolder);
        await fileSystem.Directory.CreateIfNotExistAsync(globalUserConfigFolder);
    }
    else
    {
        globalUserConfigFolder = fileSystem.Path.GetDirectoryName(globalUserConfigPath);
    }

    var configBuilder = new ConfigurationBuilder();
    _ = configBuilder.AddJsonFile("scrap.json", false, false);
    await using var s1 = await OpenAndDoIfExistsAsync(globalUserConfigPath, s => configBuilder.AddJsonStream(s));
    await using var s2 = await OpenAndDoIfExistsAsync(
        "scrap.Development.json",
        s => configBuilder.AddJsonStream(s));
    configBuilder.AddEnvironmentVariables(environmentVarPrefix);
    configBuilder.AddInMemoryCollection(
        new KeyValuePair<string, string?>[] { new("GlobalUserConfigFolder", globalUserConfigFolder) });
    var configuration = configBuilder.Build();

    var scb = new ServiceCollectionBuilder(configuration, oAuthCodeGetter);

    var registrations = scb.Build();
    registrations.AddSingleton(registrations);

    return (configuration, registrations);
        
    async Task<IAsyncDisposable> OpenAndDoIfExistsAsync(string path, Action<Stream> action)
    {
        if (!await fileSystem.File.ExistsAsync(path))
        {
            return new NullAsyncDisposable();
        }

        var stream = await fileSystem.File.OpenReadAsync(path);
        action(stream);
        return stream;
    }
}
