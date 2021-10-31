using System;
using HtmlAgilityPack;
using Scrap.Pages;

namespace Scrap.Resources.FileSystem
{
    public interface IDestinationProvider
    {
        string GetDestination(
            Uri resourceUrl,
            string destinationRootFolder,
            Page page);
    }
}