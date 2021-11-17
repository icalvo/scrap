// ReSharper disable RedundantUsingDirective
using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Scrap.Pages;
using Scrap.Resources.FileSystem.Extensions;

#pragma warning disable 1998

namespace Scrap.Resources.FileSystem
{
    // ReSharper disable once UnusedType.Global
    public class TemplateDestinationProvider: IDestinationProvider
    {
        public async Task<string> GetDestinationAsync(string destinationRootFolder,
            Page page,
            int pageIndex,
            Uri resourceUrl,
            int resourceIndex)
        {
            return "destinationFolderPattern";
        }
    }
}