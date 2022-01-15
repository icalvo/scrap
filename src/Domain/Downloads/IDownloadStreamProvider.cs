namespace Scrap.Downloads;

public interface IDownloadStreamProvider
{
    Task<Stream> GetStreamAsync(Uri url);
}