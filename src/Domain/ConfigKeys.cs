using Microsoft.Extensions.Configuration;

namespace Scrap.Domain;

public static class ConfigKeys
{
    public const string Definitions = "Scrap:Definitions";
    public const string BaseRootFolder = "Scrap:BaseRootFolder";
    public const string FileSystemType = "Scrap:FileSystemType";
    public const string Database = "Scrap:Database";
    public const string JobDefName = "JobDefinition:DefaultName";
    public const string JobDefRootUrl = "JobDefinition:DefaultRootUrl";
    public const string GlobalUserConfigPath = "Scrap:GlobalConfigPath";
}

public static class ConfigValues
{
    public static string? Definitions(this IConfiguration config) => config[ConfigKeys.Definitions];
    public static string? BaseRootFolder(this IConfiguration config) => config[ConfigKeys.BaseRootFolder];
    public static string? FileSystemType(this IConfiguration config) => config[ConfigKeys.FileSystemType];
    public static string? Database(this IConfiguration config) => config[ConfigKeys.Database];
    public static string? JobDefName(this IConfiguration config) => config[ConfigKeys.JobDefName];
    public static string? JobDefRootUrl(this IConfiguration config) => config[ConfigKeys.JobDefRootUrl];
    public static string? GlobalUserConfigPath(this IConfiguration config) => config[ConfigKeys.GlobalUserConfigPath];
}
