namespace Scrap.Domain.Jobs;

public interface IDownloadJob : IResourceRepositoryOptions, IPageRetrieverOptions
{
    public bool DownloadAlways { get; }
}