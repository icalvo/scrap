using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Scrap.Downloads;
using Scrap.Pages;
using Scrap.ResourceDownloaders;

namespace Scrap.Resources
{
    public abstract class ResourceRepositoryBase<TId> : IResourceRepository<TId>
        where TId: IResourceId
    {
        public IResourceProcessor BuildProcessor(
            IDownloadStreamProvider downloadStreamProvider,
            ILoggerFactory loggerFactory)
        {
            return new DownloadResourceProcessor<TId>(
                downloadStreamProvider,
                new Logger<DownloadResourceProcessor<TId>>(loggerFactory),
                this);
        }

        public abstract Task<TId> GetIdAsync(Page page, int pageIndex, Uri resourceUrl, int resourceIndex);
        public abstract Task<bool> ExistsAsync(TId id);
        public abstract Task UpsertAsync(TId id, Stream resourceStream);
    }
}