using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
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
    BuildCommandSetup<ScrapCommand, ScrapOptions>(), BuildCommandSetup<AllCommand, AllOptions>(),
    BuildCommandSetup<ConfigureCommand, ConfigureOptions>(),
    BuildCommandSetup<DeleteVisitedCommand, DeleteVisitedOptions>(),
    BuildCommandSetup<DownloadCommand, DownloadOptions>(), BuildCommandSetup<ResourcesCommand, ResourcesOptions>(),
    BuildCommandSetup<SearchVisitedCommand, SearchVisitedOptions>(),
    BuildCommandSetup<ShowConfigCommand, ShowConfigOptions>(), BuildCommandSetup<TraverseCommand, TraverseOptions>()
};

var optionTypes = commandSetups.Select(x => x.OptionsType).ToArray();
var parser = new Parser(settings => settings.HelpWriter = null);
var parserResult = parser.ParseArguments(args, optionTypes);
await parserResult.WithNotParsed(errors => DisplayHelp(parserResult)).WithParsedAsync(
    options => commandSetups.First(x => options.GetType() == x.OptionsType).ExecuteAsync(options));

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
            return HelpText.DefaultParsingErrorsHandler(result, h);
        });
    Console.WriteLine(helpText);
}

CommandSetup<TCommand, TOptions> BuildCommandSetup<TCommand, TOptions>()
    where TCommand : class, ICommand<TCommand, TOptions> where TOptions : OptionsBase =>
    new(cfg, sc);

async Task<(IConfiguration, IServiceCollection)> BuildServiceCollection()
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
