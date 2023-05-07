using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Scrap.Common;
using Scrap.Domain;
using Scrap.Domain.Resources.FileSystem;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class ConfigureVerb : IVerb<ConfigureVerb, ConfigureOptions>
{
    private readonly IConfiguration _configuration;
    private readonly IFileSystemFactory _fileSystemFactory;
    public const string Name = "configure";

    public ConfigureVerb(
        IConfiguration configuration,
        IFileSystemFactory fileSystemFactory)
    {
        _configuration = configuration;
        _fileSystemFactory = fileSystemFactory;
    }

    public async Task ExecuteAsync(ConfigureOptions settings)
    {
        var fileSystem = await _fileSystemFactory.BuildAsync(null);
        if (settings.Key == null)
        {
            await ConfigureInteractiveAsync(fileSystem);
        }
        else
        {
            await ConfigureNonInteractiveAsync(fileSystem, settings.Key, settings.Value);
        }
    }

    private async Task ConfigureInteractiveAsync(IFileSystem fileSystem)
    {
        Debug.Assert(_configuration != null, nameof(_configuration) + " != null");
        var globalUserConfigPath = _configuration.GlobalUserConfigPath() ?? fileSystem.DefaultGlobalUserConfigFile;
        var globalUserConfigFolder = fileSystem.Path.GetDirectoryName(globalUserConfigPath);


        await fileSystem.Directory.CreateIfNotExistAsync(globalUserConfigFolder);
        if (await fileSystem.File.ExistsAsync(globalUserConfigPath))
        {
            Console.WriteLine($"Global config file found at: {globalUserConfigPath}");
        }
        else
        {
            Console.WriteLine(
                $"Global config file not found. We are going to create a global config file and ask some values. This file is located at: {globalUserConfigPath}");
            Console.WriteLine(
                "The global config file will not be modified or deleted by any install, update or uninstall of this tool.");

            await fileSystem.File.WriteAllTextAsync(globalUserConfigPath, "{ \"Scrap\": {}}");
            Console.WriteLine($"Created global config at: {globalUserConfigPath}");
        }

        var globalUserConfigStream = await fileSystem.File.OpenReadAsync(globalUserConfigPath);
        var cfg = new ConfigurationBuilder().AddJsonStream(globalUserConfigStream).Build();

        await CreateGlobalConfigFileAsync(fileSystem, globalUserConfigFolder, globalUserConfigPath);

        var updates = GlobalConfigurations.GetGlobalConfigs(globalUserConfigFolder).Select(AskGlobalConfigValue)
            .RemoveNulls().ToArray();
        if (updates.Length == 0)
        {
            Console.WriteLine("Nothing changed!");
        }
        else
        {
            Console.WriteLine($"Adding or updating {updates.Length} config value(s)");
            var updater = new JsonUpdater(fileSystem, globalUserConfigPath);
            await updater.AddOrUpdateAsync(updates);
        }

        KeyValuePair<string, object?>? AskGlobalConfigValue(GlobalConfig globalConfig)
        {
            var (keys, defaultValue, prompt, _) = globalConfig;
            var currentValue = keys.Aggregate((string?)null, (ac, key) => ac ?? cfg[key]);
            var promptDefaultValue = currentValue ?? defaultValue;

            Console.Write($"{prompt} [{promptDefaultValue}]: ");
            var value = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(value))
            {
                value = promptDefaultValue;
            }

            if (value == currentValue)
            {
                return null;
            }

            return new KeyValuePair<string, object?>(keys[0], value);
        }
    }

    private async Task ConfigureNonInteractiveAsync(IFileSystem fileSystem, string key, string? value = null)
    {
        Debug.Assert(_configuration != null, nameof(_configuration) + " != null");
        var globalUserConfigPath = _configuration.GlobalUserConfigPath() ?? fileSystem.DefaultGlobalUserConfigFile;
        var globalUserConfigFolder = fileSystem.Path.GetDirectoryName(globalUserConfigPath);
        await fileSystem.Directory.CreateIfNotExistAsync(globalUserConfigFolder);
        if (await fileSystem.File.ExistsAsync(globalUserConfigPath))
        {
            Console.WriteLine($"Global config file found at: {globalUserConfigPath}");
        }
        else
        {
            await fileSystem.File.WriteAllTextAsync(globalUserConfigPath, "{ \"Scrap\": {}}");
            Console.WriteLine($"Created global config at: {globalUserConfigPath}");
        }

        if (value == null)
        {
            await Console.Error.WriteLineAsync("You must set a value");
            return;
        }

        Debug.Assert(key != null, $"{nameof(key)} != null");
        await CreateGlobalConfigFileAsync(fileSystem, globalUserConfigFolder, globalUserConfigPath);

        var update = GlobalConfigurations.GetGlobalConfigs(globalUserConfigFolder)
            .SingleOrDefault(x => x.Keys.Any(k => k == key));
        if (update == null)
        {
            await Console.Error.WriteLineAsync("Key not found!");
        }

        var updater = new JsonUpdater(fileSystem, globalUserConfigPath);
        await updater.AddOrUpdateAsync(new[] { new KeyValuePair<string, object?>(key, value) });
        Console.WriteLine($"{key}={value}");
    }


    private static async Task CreateGlobalConfigFileAsync(
        IFileSystem fileSystem,
        string globalUserConfigFolder,
        string globalUserConfigPath)
    {
        await fileSystem.Directory.CreateIfNotExistAsync(globalUserConfigFolder);
        if (!await fileSystem.File.ExistsAsync(globalUserConfigPath))
        {
            Console.WriteLine(
                "Global config file not found. We are going to create a global config file and ask some values. This file is located at: {globalUserConfigPath}");
            Console.WriteLine(
                "The global config file will not be modified or deleted by any install, update or uninstall of this tool.");
            await fileSystem.File.WriteAllTextAsync(globalUserConfigPath, "{ \"Scrap\": {}}");
            Console.WriteLine($"Created global config at: {globalUserConfigPath}");
        }
    }
}
