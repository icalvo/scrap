using Microsoft.Extensions.Configuration;

namespace Scrap.Domain;

public static class ConfigValues
{
    public static string? Sites(this IConfiguration config) => config[ConfigKeys.Sites];
    public static string? BaseRootFolder(this IConfiguration config) => config[ConfigKeys.BaseRootFolder];
    public static string? FileSystemType(this IConfiguration config) => config[ConfigKeys.FileSystemType];
    public static string? Database(this IConfiguration config) => config[ConfigKeys.Database];
    public static string? SiteName(this IConfiguration config) => config[ConfigKeys.SiteName];
    public static string? SiteRootUrl(this IConfiguration config) => config[ConfigKeys.SiteRootUrl];
    public static string? GlobalUserConfigPath(this IConfiguration config) => config[ConfigKeys.GlobalUserConfigPath];
}
