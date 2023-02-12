namespace Scrap.Domain.Resources;

public interface IResourceRepository
{
    string Type { get; }

    /// <summary>
    ///     Does the resource exist in the repository?
    /// </summary>
    /// <param name="resourceInfo"></param>
    /// <returns></returns>
    Task<bool> ExistsAsync(ResourceInfo resourceInfo);

    Task UpsertAsync(ResourceInfo resourceInfo, Stream resourceStream);

    Task<string> GetKeyAsync(ResourceInfo resourceInfo);
}

public interface IResourceRepository<TResourceId> : IResourceRepository where TResourceId : IResourceId
{
    Task<TResourceId> GetIdAsync(ResourceInfo resourceInfo);

    Task<bool> ExistsAsync(TResourceId id);

    Task UpsertAsync(TResourceId id, Stream resourceStream);
}
