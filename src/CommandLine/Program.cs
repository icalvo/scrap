using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Scrap.CommandLine;
using Scrap.Domain;
using Scrap.Domain.Resources.FileSystem;
using Scrap.Infrastructure;
using Scrap.Infrastructure.Factories;
using Spectre.Console;
using Spectre.Console.Cli;

var app = new CommandApp<ScrapCommand>(await BuildTypeRegistrar());
app.Configure(ConfigureCommandLine);
await app.RunAsync(args);

async Task<ITypeRegistrar> BuildTypeRegistrar()
{
    const string environmentVarPrefix = "Scrap_";
    IOAuthCodeGetter oAuthCodeGetter = new ConsoleOAuthCodeGetter();
    var configuration = new ConfigurationBuilder().AddEnvironmentVariables(environmentVarPrefix).Build();
    var fileSystemType = configuration[ConfigKeys.FileSystemType]?.ToLowerInvariant() ?? "local";

    var fileSystemFactory = new FileSystemFactory(
        oAuthCodeGetter,
        fileSystemType,
        NullLogger<FileSystemFactory>.Instance);
    var fileSystem = await fileSystemFactory.BuildAsync(false);

    var globalUserConfigPath = Global.GetGlobalUserConfigFile(configuration);
    var globalUserConfigFolder = fileSystem.Path.GetDirectoryName(globalUserConfigPath);

    var defaultUserConfigFolder = fileSystem.Path.GetDirectoryName(Global.DefaultGlobalUserConfigFile);

    if (globalUserConfigFolder == defaultUserConfigFolder)
    {
        await fileSystem.Directory.CreateIfNotExistAsync(globalUserConfigFolder);
    }

    var configBuilder = new ConfigurationBuilder();
    _ = configBuilder.AddJsonFile("scrap.json", false, false);
    await using var stream2 = await OpenAndDoIfExistsAsync(globalUserConfigPath, s => configBuilder.AddJsonStream(s));
    await using var stream3 = await OpenAndDoIfExistsAsync(
        "scrap.Development.json",
        s => configBuilder.AddJsonStream(s));
    configBuilder.AddEnvironmentVariables(environmentVarPrefix);
    configuration = configBuilder.Build();

    var registrations = new ServiceCollection();
    registrations.AddSingleton<IConfiguration>(configuration);
    registrations.AddSingleton<IFileSystem>(fileSystem);
    registrations.AddSingleton<IOAuthCodeGetter>(oAuthCodeGetter);
// Create a type registrar and register any dependencies.
// A type registrar is an adapter for a DI framework.
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
    config.AddCommand<ScrapCommand>("scrap").WithAlias("s").WithDescription("Executes a job definition");
    config.AddCommand<AllCommand>("all").WithDescription("Executes all job definitions with a root URL");
    config.AddCommand<ConfigureCommand>("configure").WithAlias("c").WithAlias("config").WithDescription("Configures the tool");
    config.AddCommand<TraverseCommand>("traverse").WithAlias("t").WithDescription("Lists all the pages reachable with the adjacency path");
    config.AddCommand<ResourcesCommand>("resources").WithAlias("r").WithDescription("Lists all the resources available in pages provided by console input");
    config.AddCommand<DownloadCommand>("download").WithAlias("d").WithDescription("Downloads resources as given by the console input");
    config.AddCommand<MarkVisitedCommand>("markvisited").WithAlias("m").WithAlias("mv").WithDescription("Adds a visited page");
    config.AddCommand<SearchVisitedCommand>("searchvisited").WithAlias("sv").WithDescription("Searches visited pages");
    config.AddCommand<DeleteVisitedCommand>("deletevisited").WithAlias("dv").WithDescription("Searches and removes visited pages");
    config.AddCommand<ShowConfigCommand>("showconfig").WithAlias("sc").WithDescription("Show configuration");
    config.AddCommand<VersionCommand>("version").WithAlias("v").WithDescription("Show version");
}
