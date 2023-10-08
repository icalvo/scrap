using Microsoft.Extensions.Logging;
using Scrap.Common;
using Scrap.Domain.Resources;
using SharpX;

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
        RootUrl = rootUrl.ToMaybe2();
        AdjacencyXPath = adjacencyXPath.ToMaybe2();
        ResourceXPath = resourceXPath.ToMaybe2();
        ResourceRepoArgs = resourceRepoArgs.ToMaybe2();
        HttpRequestRetries = httpRequestRetries.ToMaybe3();
        HttpRequestDelayBetweenRetries = httpRequestDelayBetweenRetries.ToMaybe3();
        UrlPattern = urlPattern.ToMaybe2();
        ResourceType = resourceType ?? ResourceType.DownloadLink;
    }

    public string Name { get; }
    public Maybe<Uri> RootUrl { get; }
    public Maybe<XPath> AdjacencyXPath { get; }
    public Maybe<XPath> ResourceXPath { get; }
    public Maybe<IResourceRepositoryConfiguration> ResourceRepoArgs { get; }
    public Maybe<int> HttpRequestRetries { get; }
    public Maybe<TimeSpan> HttpRequestDelayBetweenRetries { get; }
    public Maybe<string> UrlPattern { get; }
    public ResourceType ResourceType { get; }

    public bool HasResourceCapabilities() => ResourceXPath.IsJust() && ResourceRepoArgs.IsJust();

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
