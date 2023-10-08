namespace Scrap.Domain.Jobs;

class ResourcesJob : IResourcesJob
{
    public ResourcesJob(int httpRequestRetries, TimeSpan httpRequestDelayBetweenRetries, XPath resourceXPath)
    {
        HttpRequestRetries = httpRequestRetries;
        HttpRequestDelayBetweenRetries = httpRequestDelayBetweenRetries;
        ResourceXPath = resourceXPath;
    }

    public int HttpRequestRetries { get; }
    public TimeSpan HttpRequestDelayBetweenRetries { get; }
    public XPath ResourceXPath { get; }
}