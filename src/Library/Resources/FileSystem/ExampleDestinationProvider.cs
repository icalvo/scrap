using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Scrap.Pages;
using Scrap.Resources.FileSystem.Extensions;

namespace Scrap.Resources.FileSystem
{
    public class InternalDestinationProvider: IDestinationProvider
    {
        public async Task<string> GetDestinationAsync(
            Uri resourceUrl,
            string destinationRootFolder,
            Page page, 
            int pageIndex)
        {
            return "destinationFolderPattern";
        }
    }
}