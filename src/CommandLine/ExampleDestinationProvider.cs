using System;
using System.IO;
using System.Net;
using System.Linq;
using HtmlAgilityPack;
using Microsoft.CodeAnalysis;
using Scrap.CommandLine;

namespace RoslynCompileSample
{
    public class InternalDestinationProvider2 : BaseDestinationProvider
    {
        public InternalDestinationProvider2(IPageRetriever pageRetriever, Uri baseUri)
            : base(pageRetriever, baseUri)
        {
        }

        public override string GetDestination(
            Uri resourceUrl,
            string destinationRootFolder,
            Uri pageUrl,
            HtmlDocument pageDoc)
        {
            return
                destinationRootFolder.C(pageDoc.GetContent("//h1") ?? "").C(pageUrl.CleanSegments()[^1] + resourceUrl.Extension()).Combine();
        }
    }
}