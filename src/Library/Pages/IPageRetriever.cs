using System;

namespace Scrap.Pages
{
    public interface IPageRetriever
    {
        Page GetPage(Uri uri);
    }
}