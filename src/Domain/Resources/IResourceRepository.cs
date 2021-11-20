using System;
using System.IO;
using System.Threading.Tasks;
using Scrap.Pages;

namespace Scrap.Resources
{
    public class ResourceInfo
    {
        public ResourceInfo(Page page, int pageIndex, Uri resourceUrl, int resourceIndex)
        {
            this.Page = page;
            this.PageIndex = pageIndex;
            this.ResourceUrl = resourceUrl;
            this.ResourceIndex = resourceIndex;
        }
        public Page Page { get; }
        public int PageIndex { get; }
        public Uri ResourceUrl { get; }
        public int ResourceIndex { get; }

        public void Deconstruct(out Page page, out int pageIndex, out Uri resourceUrl, out int resourceIndex)
        {
            page = this.Page;
            pageIndex = this.PageIndex;
            resourceUrl = this.ResourceUrl;
            resourceIndex = this.ResourceIndex;
        }
    }
    public interface IResourceRepository
    {
        Task<bool> ExistsAsync(ResourceInfo resourceInfo);

        Task UpsertAsync(ResourceInfo resourceInfo, Stream resourceStream);

        Task<string> GetKeyAsync(ResourceInfo resourceInfo);
    }    

    public interface IResourceRepository<TResourceId>: IResourceRepository
        where TResourceId: IResourceId
    {
        Task<TResourceId> GetIdAsync(ResourceInfo resourceInfo);
        
        Task<bool> ExistsAsync(TResourceId id);

        Task UpsertAsync(TResourceId id, Stream resourceStream);
    }

    public abstract class BaseResourceRepository<TResourceId> : IResourceRepository<TResourceId>
        where TResourceId : IResourceId
    {
        public async Task<bool> ExistsAsync(ResourceInfo resourceInfo)
        {
            var id = await GetIdAsync(resourceInfo);
            return await ExistsAsync(id);
        }
        public async Task UpsertAsync(ResourceInfo resourceInfo, Stream resourceStream)
        {
            var id = await GetIdAsync(resourceInfo);
            await UpsertAsync(id, resourceStream);
        }

        public virtual async Task<string> GetKeyAsync(ResourceInfo resourceInfo)
        {
            return (await GetIdAsync(resourceInfo)).ToString();
        }

        public abstract Task<TResourceId> GetIdAsync(ResourceInfo resourceInfo);
        public abstract Task<bool> ExistsAsync(TResourceId id);
        public abstract Task UpsertAsync(TResourceId id, Stream resourceStream);
    }
}
