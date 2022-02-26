using Scrap.Domain.Pages;

namespace Scrap.Domain;

public interface ILinkCalculator
{
    IAsyncEnumerable<Uri> CalculateLinks(
        IPage page,
        XPath? adjacencyXPath);
}