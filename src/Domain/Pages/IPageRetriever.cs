using System;
using System.Threading.Tasks;

namespace Scrap.Pages;

public interface IPageRetriever
{
    Task<Page> GetPageAsync(Uri uri);
}