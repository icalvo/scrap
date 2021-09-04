using System;
using HtmlAgilityPack;

namespace Scrap.CommandLine
{
    public interface IDestinationProvider
    {
        string GetDestination(
            Uri resourceUrl,
            string destinationRootFolder,
            Uri pageUrl,
            HtmlDocument pageDoc);
    }
}