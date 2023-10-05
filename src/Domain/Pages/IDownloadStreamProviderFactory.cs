using Scrap.Domain.Downloads;
using Scrap.Domain.Jobs;

namespace Scrap.Domain.Pages;

public interface IDownloadStreamProviderFactory
{
    public IDownloadStreamProvider Build(IDownloadStreamProviderOptions job);
}
