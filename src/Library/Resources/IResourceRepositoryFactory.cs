using Polly;

namespace Scrap.Resources
{
    public interface IResourceRepositoryFactory
    {
        IResourceRepository Build(IAsyncPolicy httpPolicy, IResourceRepositoryConfiguration args, bool whatIf);
    }
}