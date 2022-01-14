using System;
using System.Collections.Generic;
using System.Xml.XPath;
using HtmlAgilityPack;

namespace Scrap;

public static class HtmlDocumentExtensions
{
        
    public static IEnumerable<string?> Contents(this HtmlDocument doc, XPath xpath)
    {
        XPathNavigator nav = doc.CreateNavigator() ?? throw new Exception();
        var sel3 = nav.Select(xpath);
        if (xpath.IsHtml)
        {
            while (sel3.MoveNext())
            {
                yield return sel3.Current?.InnerXml;
            }
        }
        else
        {
            while (sel3.MoveNext())
            {
                yield return sel3.Current?.Value;
            }
        }
    }
}
