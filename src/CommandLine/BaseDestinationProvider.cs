using System;
using HtmlAgilityPack;

namespace Scrap.CommandLine
{
    public abstract class BaseDestinationProvider : IDestinationProvider
    {
        private readonly IPageRetriever _pageRetriever;
        private readonly Uri _baseUri;
        protected BaseDestinationProvider(IPageRetriever pageRetriever, Uri baseUri)
        {
            _pageRetriever = pageRetriever;
            _baseUri = baseUri;
        }

        protected HtmlDocument? Doc(HtmlDocument pageDoc, string linkXPath)
        {
            var link = pageDoc.GetLink(linkXPath, "href", _baseUri);
            return link == null ? null : _pageRetriever.GetPage(link).Document;
        }

        public abstract string GetDestination(
            Uri resourceUrl,
            string destinationRootFolder,
            Uri pageUrl,
            HtmlDocument pageDoc);
    }
}