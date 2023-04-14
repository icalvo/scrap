using Scrap.Domain;

namespace Scrap.CommandLine;

public static class GlobalConfigurations
{
    public static IEnumerable<GlobalConfig> GetGlobalConfigs(string globalUserConfigFolder) =>
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
            new GlobalConfig(ConfigKeys.FileSystemType, "local", "Filesystem type (local/dropbox)", true),
            new GlobalConfig(
                ConfigKeys.BaseRootFolder,
                null,
                "Base download path for your file-based resource repository",
                true)
        };
}
