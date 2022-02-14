using Scrap.Pages;

namespace Scrap.Resources.FileSystem;

public interface IDestinationProvider
{
    Task<string> GetDestinationAsync(
        string rootFolder,
        IPage page,
        int pageIndex,
        Uri resourceUrl,
        int resourceIndex);
}