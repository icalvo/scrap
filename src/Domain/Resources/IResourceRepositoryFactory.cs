using Scrap.Common;
using Scrap.Domain.Jobs;

namespace Scrap.Domain.Resources;

public interface IResourceRepositoryFactory : IAsyncFactory<Job, IResourceRepository> { }