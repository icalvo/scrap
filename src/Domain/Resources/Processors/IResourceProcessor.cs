using System;
using System.Threading.Tasks;
using Scrap.Pages;

namespace Scrap.ResourceDownloaders
{
    public interface IResourceProcessor
    {
        Task DownloadResourceAsync(
            Page page,
            int pageIndex,
            Uri resourceUrl,
            int resourceIndex);
    }
}