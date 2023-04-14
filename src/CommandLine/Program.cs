using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Scrap.CommandLine;
using Scrap.Domain;
using Scrap.Infrastructure;
using Scrap.Infrastructure.Factories;
using Spectre.Console;
using Spectre.Console.Cli;

TaskScheduler.UnobservedTaskException += (_, eventArgs) =>
    Console.Error.WriteLine($"Unobserved exception: {eventArgs.Exception}");

var app = new CommandApp<ScrapCommand>(await BuildTypeRegistrar());
app.Configure(ConfigureCommandLine);
await app.RunAsync(args);

async Task<ITypeRegistrar> BuildTypeRegistrar()
{
    const string environmentVarPrefix = "Scrap_";
    IOAuthCodeGetter oAuthCodeGetter = new ConsoleOAuthCodeGetter();
    var configuration = new ConfigurationBuilder().AddEnvironmentVariables(environmentVarPrefix).Build();
    var fileSystemType = configuration.FileSystemType()?.ToLowerInvariant() ?? "local";

    var fileSystemFactory = new FileSystemFactory(
        oAuthCodeGetter,
        fileSystemType,
        NullLogger<FileSystemFactory>.Instance);
    var fileSystem = await fileSystemFactory.BuildAsync(false);

    var globalUserConfigPath = configuration.GlobalUserConfigPath();
    string globalUserConfigFolder;

    if (globalUserConfigPath == null)
    {
        globalUserConfigPath = fileSystem.DefaultGlobalUserConfigFile;
        globalUserConfigFolder = fileSystem.Path.GetDirectoryName(globalUserConfigPath);
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
    configuration = configBuilder.Build();

    var scb = new ServiceCollectionBuilder(configuration, oAuthCodeGetter);

    var registrations = scb.Build();
    registrations.AddSingleton(registrations);
    registrations.AddSingleton<IJobDtoBuilder, JobDtoBuilder>();
    var typeRegistrar = new TypeRegistrar(registrations);
    return typeRegistrar;
    
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

void ConfigureCommandLine(IConfigurator config)
{
#if DEBUG
    config.ValidateExamples();
#endif
    config.Settings.ApplicationName = "scrap";
    config.SetExceptionHandler(
        ex =>
        {
            if (ex is CommandRuntimeException)
                AnsiConsole.WriteLine(ex.Message);
            else
            {
                AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything | ExceptionFormats.ShowLinks);
            }

            return -99;
        });

    config.AddCommand<ConfigCheckedServiceResolvedCommand<ScrapCommand, ScrapSettings>>("scrap").WithAlias("s")
        .WithDescription("Executes a job definition");
    config.AddCommand<ConfigCheckedServiceResolvedCommand<AllCommand, AllSettings>>("all")
        .WithDescription("Executes all job definitions with a root URL");
    config.AddCommand<ServiceResolverCommand<ConfigureCommand, ConfigureSettings>>(ConfigureCommand.Name).WithAlias("c")
        .WithAlias("config")
        .WithDescription("Configures the tool");
    config.AddCommand<ConfigCheckedServiceResolvedCommand<TraverseCommand, TraverseSettings>>("traverse").WithAlias("t")
        .WithDescription("Lists all the pages reachable with the adjacency path").WithData(new CommandData(false));
    config.AddCommand<ConfigCheckedServiceResolvedCommand<ResourcesCommand, ResourcesSettings>>("resources")
        .WithAlias("r").WithDescription("Lists all the resources available in pages provided by console input")
        .WithData(new CommandData(false));
    config.AddCommand<ConfigCheckedServiceResolvedCommand<DownloadCommand, DownloadSettings>>("download").WithAlias("d")
        .WithDescription("Downloads resources as given by the console input");
    config.AddCommand<ConfigCheckedServiceResolvedCommand<MarkVisitedCommand, MarkVisitedSettings>>("markvisited")
        .WithAlias("m").WithAlias("mv").WithDescription("Adds a visited page");
    config.AddCommand<ConfigCheckedServiceResolvedCommand<SearchVisitedCommand, SearchSettings>>("searchvisited")
        .WithAlias("sv").WithDescription("Searches visited pages").WithData(new CommandData(false));
    config.AddCommand<ConfigCheckedServiceResolvedCommand<DeleteVisitedCommand, SearchSettings>>("deletevisited")
        .WithAlias("dv").WithDescription("Searches and removes visited pages");
    config.AddCommand<ConfigCheckedServiceResolvedCommand<ShowConfigCommand, ShowConfigSettings>>("showconfig")
        .WithAlias("sc").WithDescription("Show configuration").WithData(new CommandData(false));
    ;
    config.AddCommand<VersionCommand>("version").WithAlias("v").WithDescription("Show version")
        .WithData(new CommandData(false));
}

public record CommandData(bool ConsoleLog);

internal class ConfigCheckedServiceResolvedCommand<TRawCommand, TSettings> : AsyncCommand<TSettings>
    where TRawCommand : class, ICommand<TSettings> where TSettings : SettingsBase
{
    private readonly ICommand<TSettings> _commandImplementation;

    public ConfigCheckedServiceResolvedCommand(
        IGlobalConfigurationChecker globalConfigurationChecker,
        IConfiguration configuration,
        IServiceCollection serviceCollection)
    {
        _commandImplementation = new ConfigurationCheckedCommand<TSettings>(
            new ServiceResolverCommand<TRawCommand, TSettings>(configuration, serviceCollection),
            globalConfigurationChecker);
    }

    public override Task<int> ExecuteAsync(CommandContext context, TSettings settings) =>
        _commandImplementation.Execute(context, settings);
}
