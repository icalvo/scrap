using System;
using System.IO;
using System.Net;
using System.Linq;
using HtmlAgilityPack;
using Scrap.CommandLine;

namespace RoslynCompileSample
{
    public class InternalDestinationProvider2 : IDestinationProvider
    {
        public string GetDestination(
            Uri resourceUrl,
            string destinationRootFolder,
            Uri pageUrl,
            HtmlDocument pageDoc)
        {
            return "";
        }
    }
}