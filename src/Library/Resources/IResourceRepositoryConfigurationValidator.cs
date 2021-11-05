using Polly;

namespace Scrap.Resources
{
    public interface IResourceRepositoryConfigurationValidator
    {
        void Validate(IResourceRepositoryConfiguration args);
    }
}