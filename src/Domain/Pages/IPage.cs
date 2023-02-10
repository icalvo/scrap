using System.Xml.XPath;

namespace Scrap.Domain.Pages;

public interface IPage : IEquatable<IPage>
{
    Uri Uri { get; }
    IXPathNavigable Document { get; }
    IEnumerable<Uri> Links(XPath xPath);
    Uri? Link(XPath xPath);
    string Concat(XPath xPath);
    IEnumerable<string?> Contents(XPath xPath);
    string Content(XPath xPath);
    Task<IPage?> LinkedDoc(string xPath);
    string? ContentOrNull(XPath xPath);
    Task<IPage> RecreateAsync();
}
