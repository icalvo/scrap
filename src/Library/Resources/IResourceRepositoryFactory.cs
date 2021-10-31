using Microsoft.Extensions.Logging;

namespace Scrap.Resources
{
    public interface IResourceRepositoryFactory
    {
        IResourceRepository Build(string id, params string[] args);
    }
}