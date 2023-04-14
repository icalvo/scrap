using Microsoft.Extensions.Configuration;
using Scrap.Domain;
using Scrap.Domain.Resources.FileSystem;

namespace Scrap.CommandLine;

public class GlobalConfigurationChecker : IGlobalConfigurationChecker
{
    private readonly IConfiguration _configuration;
    private readonly IFileSystem _fileSystem;

    public GlobalConfigurationChecker(IConfiguration configuration, IFileSystem fileSystem)
    {
        _configuration = configuration;
        _fileSystem = fileSystem;
    }

    public async Task EnsureGlobalConfigurationAsync()
    {
        var globalUserConfigPath = _configuration.GlobalUserConfigPath() ?? _fileSystem.DefaultGlobalUserConfigFile;
        if (!await _fileSystem.File.ExistsAsync(globalUserConfigPath))
        {
            Console.WriteLine($"The global config file [{globalUserConfigPath}] does not exist");
            throw new ScrapException("The tool is not properly configured; call 'scrap config'");
        }

        var globalUserConfigFolder = _fileSystem.Path.GetDirectoryName(globalUserConfigPath);
        var unsetKeys = GlobalConfigurations.GetGlobalConfigs(globalUserConfigFolder)
            .Where(config => !config.Optional && _configuration[config.Key] == null).ToArray();
        if (!unsetKeys.Any())
        {
            return;
        }

        var keyList = string.Join(", ", unsetKeys.Select(x => x.Key));
        Console.WriteLine($"Unset configuration keys: {keyList}");
        throw new ScrapException("The tool is not properly configured; call 'scrap config'");
    }
}
