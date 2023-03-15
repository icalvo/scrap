using Scrap.Common;
using Scrap.Domain.Resources;

namespace Scrap.Domain.Jobs;

public interface IResourceRepositoryConfigurationValidatorFactory
    : IAsyncFactory<IResourceRepositoryConfiguration, IResourceRepositoryConfigurationValidator> {}
