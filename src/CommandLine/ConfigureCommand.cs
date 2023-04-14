﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrap.Common;
using Scrap.Domain;
using Scrap.Domain.Resources.FileSystem;

namespace Scrap.CommandLine;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class ConfigureCommand : AsyncCommandBase<ConfigureSettings>
{
    private readonly IFileSystemFactory _fileSystemFactory;
    public const string Name = "configure";

    public ConfigureCommand(
        IConfiguration configuration,
        IFileSystemFactory fileSystemFactory,
        IServiceCollection serviceCollection) : base(configuration)
    {
        _fileSystemFactory = fileSystemFactory;
    }

    protected override async Task<int> CommandExecuteAsync(ConfigureSettings settings)
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

        return 0;
    }

    private async Task ConfigureInteractiveAsync(IFileSystem fileSystem)
    {
        ConsoleTools.PrintHeader();

        Debug.Assert(Configuration != null, nameof(Configuration) + " != null");
        var globalUserConfigPath = Configuration.GlobalUserConfigPath() ?? fileSystem.DefaultGlobalUserConfigFile;
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

    private async Task ConfigureNonInteractiveAsync(IFileSystem fileSystem, string key, string? value = null)
    {
        Debug.Assert(Configuration != null, nameof(Configuration) + " != null");
        var globalUserConfigPath = Configuration.GlobalUserConfigPath() ?? fileSystem.DefaultGlobalUserConfigFile;
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

        var update = GlobalConfigurations.GetGlobalConfigs(globalUserConfigFolder).SingleOrDefault(x => x.Key == key);
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
