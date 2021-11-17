using System;
using System.IO;
using System.Threading.Tasks;
using Scrap.Pages;
using Scrap.ResourceDownloaders;

namespace Scrap.Resources
{
    public class ListResourceProcessor : IResourceProcessor
    {
        public Task DownloadResourceAsync(Page page, int pageIndex, Uri resourceUrl, int resourceIndex)
        {
            Console.WriteLine(resourceUrl);
            return Task.CompletedTask;
            // return _writer.WriteLineAsync(resourceUrl.AbsoluteUri);
        }
    }
}