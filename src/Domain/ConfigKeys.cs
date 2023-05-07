using Microsoft.Extensions.Configuration;

namespace Scrap.Domain;

public static class ConfigKeys
{
    public const string Sites = "Scrap:Sites";
    public const string BaseRootFolder = "Scrap:BaseRootFolder";
    public const string FileSystemType = "Scrap:FileSystemType";
    public const string Database = "Scrap:Database";
    public const string SiteName = "Site:DefaultName";
    public const string SiteRootUrl = "Site:DefaultRootUrl";
    public const string GlobalUserConfigPath = "Scrap:GlobalConfigPath";
}

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
