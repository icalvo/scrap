using System.Collections;
using System.Xml.XPath;

namespace Scrap.Domain;

public static class XPathNavigableExtensions
{
    public static IEnumerable<string?> Contents(this IXPathNavigable doc, XPath xpath)
    {
        XPathNavigator nav = doc.CreateNavigator()
                             ?? throw new ArgumentException("Could not create XPathNavigator", nameof(doc));
        var childNodes = nav.Select(xpath).ToEnumerable();
        
        return xpath.IsHtml
            ? childNodes.Select(node => node.InnerXml)
            : childNodes.Select(node => node.Value);
    }

    /// <summary>
    /// Converts an <see cref="XPathNodeIterator"/> to a <see cref="IEnumerable{T}" />.
    /// </summary>
    /// <param name="iterator">Iterator to convert.</param>
    /// <returns></returns>
    /// <remarks>
    /// <see cref="XPathNodeIterator"/> is an old, pre-generics class which has MoveNext() and Current: T
    /// (without implementing <see cref="IEnumerator{T}"/>) and implements <see cref="IEnumerable"/>.
    /// This method will convert it to a modern <see cref="IEnumerable{T}"/>.
    /// </remarks>
    private static IEnumerable<XPathNavigator> ToEnumerable(this XPathNodeIterator iterator)
    {
        while (iterator.MoveNext())
        {
            if (iterator.Current != null)
            {
                yield return iterator.Current;
            }
        }
    }
}
