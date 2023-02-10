namespace Scrap.Domain.Pages;

public interface ILinkCalculator
{
    IAsyncEnumerable<Uri> CalculateLinks(
        IPage page,
        XPath? adjacencyXPath);
}
