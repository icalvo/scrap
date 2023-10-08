using SharpX;

namespace Scrap.Domain.Pages;

public interface ILinkCalculator
{
    IAsyncEnumerable<Uri> CalculateLinks(IPage page, Maybe<XPath> adjacencyXPath);
}
