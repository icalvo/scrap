namespace Scrap.Domain.Downloads;

public interface IDownloadStreamProvider
{
    Task<Stream> GetStreamAsync(Uri url);
}