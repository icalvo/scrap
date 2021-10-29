using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace Scrap.CommandLine
{
    public static class HtmlNodeExtensions
    {
        public static IEnumerable<HtmlNode> SelectNodesBetter(this HtmlNode nodes, string xpath)
        {
            try
            {
                return nodes.SelectNodes(xpath) ?? Enumerable.Empty<HtmlNode>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error selecting " + xpath + ": " + ex.Message);
            }

            return Enumerable.Empty<HtmlNode>();
        }
    }
}