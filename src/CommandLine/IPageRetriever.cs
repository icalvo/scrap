using System;

namespace Scrap.CommandLine
{
    public interface IPageRetriever
    {
        Page GetPage(Uri uri);
    }
}