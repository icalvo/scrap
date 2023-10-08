namespace Scrap.Domain.Jobs;

public interface IResourcesJob : IPageRetrieverOptions
{
    public XPath ResourceXPath { get; }
}