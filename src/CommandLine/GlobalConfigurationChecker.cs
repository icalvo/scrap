using Microsoft.Extensions.Configuration;
using Scrap.Domain;
using Scrap.Domain.Resources.FileSystem;

namespace Scrap.CommandLine;

public class GlobalConfigurationChecker : IGlobalConfigurationChecker
{
    private readonly IConfiguration _configuration;
    private readonly IFileSystemFactory _fileSystemFactory;

    public GlobalConfigurationChecker(IConfiguration configuration, IFileSystemFactory fileSystemFactory)
    {
        _configuration = configuration;
        _fileSystemFactory = fileSystemFactory;
    }

    public async Task EnsureGlobalConfigurationAsync()
    {
        var fileSystem = await _fileSystemFactory.BuildAsync(false);
        var globalUserConfigPath = _configuration.GlobalUserConfigPath() ?? fileSystem.DefaultGlobalUserConfigFile;
        if (!await fileSystem.File.ExistsAsync(globalUserConfigPath))
        {
            Console.WriteLine($"The global config file [{globalUserConfigPath}] does not exist");
            throw new ScrapException("The tool is not properly configured; call 'scrap config'");
        }

        var globalUserConfigFolder = fileSystem.Path.GetDirectoryName(globalUserConfigPath);
        var unsetKeys = GlobalConfigurations.GetGlobalConfigs(globalUserConfigFolder).Where(
            config => !config.Optional && config.Keys.All(key => _configuration[key] == null)).ToArray();
        if (!unsetKeys.Any())
        {
            return;
        }

        var keyList = string.Join(", ", unsetKeys.Select(x => x.Keys[0]));
        Console.WriteLine($"Unset configuration keys: {keyList}");
        throw new ScrapException("The tool is not properly configured; call 'scrap config'");
    }
}
