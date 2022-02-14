using Scrap.Pages;

namespace Scrap;

public interface ILinkCalculator
{
    IAsyncEnumerable<Uri> CalculateLinks(
        IPage page,
        XPath? adjacencyXPath);
}