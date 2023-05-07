using Microsoft.Extensions.Logging;
using Scrap.Domain.Resources;

namespace Scrap.Domain.Sites;

public class Site
{
    public Site(
        string name,
        ResourceType? resourceType = null,
        Uri? rootUrl = null,
        XPath? adjacencyXPath = null,
        XPath? resourceXPath = null,
        IResourceRepositoryConfiguration? resourceRepoArgs = null,
        int? httpRequestRetries = null,
        TimeSpan? httpRequestDelayBetweenRetries = null,
        string? urlPattern = null)
    {
        Name = name;
        RootUrl = rootUrl;
        AdjacencyXPath = adjacencyXPath;
        ResourceXPath = resourceXPath;
        ResourceRepoArgs = resourceRepoArgs;
        HttpRequestRetries = httpRequestRetries;
        HttpRequestDelayBetweenRetries = httpRequestDelayBetweenRetries;
        UrlPattern = urlPattern;
        ResourceType = resourceType ?? ResourceType.DownloadLink;
    }

    public string Name { get; }
    public Uri? RootUrl { get; }
    public XPath? AdjacencyXPath { get; }
    public XPath? ResourceXPath { get; }
    public IResourceRepositoryConfiguration? ResourceRepoArgs { get; }
    public int? HttpRequestRetries { get; }
    public TimeSpan? HttpRequestDelayBetweenRetries { get; }
    public string? UrlPattern { get; }
    public ResourceType ResourceType { get; }

    public bool HasResourceCapabilities() => ResourceXPath != null && ResourceRepoArgs != null;

    public void Log(ILogger logger, LogLevel logLevel)
    {
        logger.Log(logLevel, "Name: {Name}", Name);
        logger.Log(logLevel, "Root URL: {RootUrl}", RootUrl);
        logger.Log(logLevel, "Adjacency XPath: {AdjacencyXPath}", AdjacencyXPath);
        logger.Log(logLevel, "Resource XPath: {ResourceXPath}", ResourceXPath);
        logger.Log(logLevel, "Resource repo args:\n{ResourceRepoArgs}", ResourceRepoArgs);
        logger.Log(logLevel, "Url Pattern: {UrlPattern}", UrlPattern);
        logger.Log(logLevel, "Resource Type: {ResourceType}", ResourceType);
    }

    public override string ToString() => $"Site({Name})";
}
