using System;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Scrap.Pages;

namespace Scrap.Resources.FileSystem
{
    public interface IDestinationProvider
    {
        Task<string> GetDestinationAsync(
            Uri resourceUrl,
            string destinationRootFolder,
            Page page, 
            int pageIndex);
    }
}