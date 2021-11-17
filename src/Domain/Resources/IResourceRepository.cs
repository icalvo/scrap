using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Scrap.Downloads;
using Scrap.Pages;
using Scrap.ResourceDownloaders;

namespace Scrap.Resources
{
    public interface IResourceRepository<TResourceId>
        where TResourceId: IResourceId
    {
        Task<TResourceId> GetIdAsync(
            Page page,
            int pageIndex,
            Uri resourceUrl,
            int resourceIndex);
        
        Task<bool> ExistsAsync(TResourceId id);

        Task UpsertAsync(TResourceId id, Stream resourceStream);
    }
}