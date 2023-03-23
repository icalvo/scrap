using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Scrap.Common;
using Scrap.Domain.Resources.FileSystem;
using Scrap.Infrastructure;

namespace Scrap.CommandLine;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class ConfigureCommand : AsyncCommandBase<ConfigureSettings>
{
    public const string Name = "configure";

    public ConfigureCommand(IConfiguration configuration, IOAuthCodeGetter oAuthCodeGetter, IFileSystem fileSystem)
        : base(configuration, oAuthCodeGetter, fileSystem)
    {
    }

    protected override async Task<int> CommandExecuteAsync(ConfigureSettings settings)
    {
        if (settings.Key == null)
        {
            await ConfigureInteractiveAsync();
        }
        else
        {
            await ConfigureNonInteractiveAsync(settings.Key, settings.Value);
        }

        return 0;
    }

    private async Task ConfigureInteractiveAsync()
    {
        PrintHeader();

        Debug.Assert(Configuration != null, nameof(Configuration) + " != null");
        var globalUserConfigPath = Global.GetGlobalUserConfigFile(Configuration);
        var globalUserConfigFolder = FileSystem.Path.GetDirectoryName(globalUserConfigPath);


        await FileSystem.Directory.CreateIfNotExistAsync(globalUserConfigFolder);
        if (await FileSystem.File.ExistsAsync(globalUserConfigPath))
        {
            Console.WriteLine($"Global config file found at: {globalUserConfigPath}");
        }
        else
        {
            Console.WriteLine(
                $"Global config file not found. We are going to create a global config file and ask some values. This file is located at: {globalUserConfigPath}");
            Console.WriteLine(
                "The global config file will not be modified or deleted by any install, update or uninstall of this tool.");

            await FileSystem.File.WriteAllTextAsync(globalUserConfigPath, "{ \"Scrap\": {}}");
            Console.WriteLine($"Created global config at: {globalUserConfigPath}");
        }

        var globalUserConfigStream = await FileSystem.File.OpenReadAsync(globalUserConfigPath);
        var cfg = new ConfigurationBuilder().AddJsonStream(globalUserConfigStream).Build();

        await CreateGlobalConfigFileAsync(globalUserConfigFolder, globalUserConfigPath);

        var updates = GetGlobalConfigs(globalUserConfigFolder).Select(AskGlobalConfigValue).RemoveNulls().ToArray();
        if (updates.Length == 0)
        {
            Console.WriteLine("Nothing changed!");
        }
        else
        {
            Console.WriteLine($"Adding or updating {updates.Length} config value(s)");
            var updater = new JsonUpdater(FileSystem, globalUserConfigPath);
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
        Debug.Assert(Configuration != null, nameof(Configuration) + " != null");
        var globalUserConfigPath = Global.GetGlobalUserConfigFile(Configuration);
        var globalUserConfigFolder = FileSystem.Path.GetDirectoryName(globalUserConfigPath);
        await FileSystem.Directory.CreateIfNotExistAsync(globalUserConfigFolder);
        if (await FileSystem.File.ExistsAsync(globalUserConfigPath))
        {
            Console.WriteLine($"Global config file found at: {globalUserConfigPath}");
        }
        else
        {
            await FileSystem.File.WriteAllTextAsync(globalUserConfigPath, "{ \"Scrap\": {}}");
            Console.WriteLine($"Created global config at: {globalUserConfigPath}");
        }

        if (value == null)
        {
            await Console.Error.WriteLineAsync("You must set a value");
            return;
        }

        Debug.Assert(key != null, $"{nameof(key)} != null");
        await CreateGlobalConfigFileAsync(globalUserConfigFolder, globalUserConfigPath);

        var update = GetGlobalConfigs(globalUserConfigFolder).SingleOrDefault(x => x.Key == key);
        if (update == null)
        {
            await Console.Error.WriteLineAsync("Key not found!");
        }

        var updater = new JsonUpdater(FileSystem, globalUserConfigPath);
        await updater.AddOrUpdateAsync(new[] { new KeyValuePair<string, object?>(key, value) });
        Console.WriteLine($"{key}={value}");
    }


    private async Task CreateGlobalConfigFileAsync(string globalUserConfigFolder, string globalUserConfigPath)
    {
        await FileSystem.Directory.CreateIfNotExistAsync(globalUserConfigFolder);
        if (!await FileSystem.File.ExistsAsync(globalUserConfigPath))
        {
            Console.WriteLine(
                "Global config file not found. We are going to create a global config file and ask some values. This file is located at: {globalUserConfigPath}");
            Console.WriteLine(
                "The global config file will not be modified or deleted by any install, update or uninstall of this tool.");
            await FileSystem.File.WriteAllTextAsync(globalUserConfigPath, "{ \"Scrap\": {}}");
            Console.WriteLine($"Created global config at: {globalUserConfigPath}");
        }
    }
}
