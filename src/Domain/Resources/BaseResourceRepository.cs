namespace Scrap.Domain.Resources;

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
        return (await GetIdAsync(resourceInfo)).ToText();
    }

    public string Type => this.GetType().Name;

    public abstract Task<TResourceId> GetIdAsync(ResourceInfo resourceInfo);
    public abstract Task<bool> ExistsAsync(TResourceId id);
    public abstract Task UpsertAsync(TResourceId id, Stream resourceStream);
}
