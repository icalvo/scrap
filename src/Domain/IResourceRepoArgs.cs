using Scrap.Domain.Resources;

namespace Scrap.Domain;

public interface IResourceRepoArgs
{
    IResourceRepositoryConfiguration ResourceRepoArgs { get; }
}
