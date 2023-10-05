using Scrap.Common;
using Scrap.Domain.Downloads;
using Scrap.Domain.Jobs;

namespace Scrap.Domain.Pages;

public interface IDownloadStreamProviderFactory : IFactory<Job, IDownloadStreamProvider> {}