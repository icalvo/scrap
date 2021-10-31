using System;
using System.IO;
using System.Threading.Tasks;

namespace Scrap.Resources
{
    public class NullResourceDownloader: IResourceDownloader
    {
        public Task DownloadFileAsync(Uri uri, Stream outputStream)
        {
            return Task.CompletedTask;
        }
    }
}