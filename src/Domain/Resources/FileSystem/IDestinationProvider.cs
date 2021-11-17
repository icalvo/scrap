using System;
using System.Threading.Tasks;
using Scrap.Pages;

namespace Scrap.Resources.FileSystem
{
    public interface IDestinationProvider
    {
        Task<string> GetDestinationAsync(string destinationRootFolder,
            Page page,
            int pageIndex, Uri resourceUrl,
            int resourceIndex);
    }
}