using SharpX;

namespace Scrap.Domain.Jobs;

public interface ITraverseJob : IPageRetrieverOptions, ILinkCalculatorOptions
{
    public Uri RootUrl { get; }
    public Maybe<XPath> AdjacencyXPath { get; }
}
