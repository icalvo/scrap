using System;
using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;

namespace Scrap.CommandLine
{
    public class CachedPageRetriever : IPageRetriever
    {
        private readonly HtmlWeb _web = new();
        private readonly Dictionary<Uri, Page> _pages = new();

        public Page GetPage(Uri uri)
        {
            if (_pages.TryGetValue(uri, out var page))
            {
                Console.WriteLine("CACHED {0}", uri);
                return page;
            }

            while (true)
            {
                try
                {
                    Thread.Sleep(1000);
                    Console.WriteLine("GET {0}", uri);
                    page = new Page(uri, _web.Load(uri.AbsoluteUri));
                    _pages.Add(uri, page);
                    return page;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }            
        }
    }
}