using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace Scrap.CommandLine
{
    public static class HtmlNodeExtensions
    {
        public static IEnumerable<HtmlNode> SelectNodesBetter(this HtmlNode nodes, string xpath)
        {
            return nodes.SelectNodes(xpath) ?? Enumerable.Empty<HtmlNode>();
        }
    }
}