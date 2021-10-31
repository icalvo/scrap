using System;
using System.IO;
using System.Threading.Tasks;

namespace Scrap.Resources
{
    public interface IResourceDownloader
    {
        Task DownloadFileAsync(
            Uri uri,
            Stream outputStream);
    }
}