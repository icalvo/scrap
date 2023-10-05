using Scrap.Domain.Pages;

namespace Scrap.Domain.Resources.FileSystem;

public interface IDestinationProvider
{
    Task ValidateAsync();

    Task<string> GetDestinationAsync(string rootFolder, IPage page, int pageIndex, Uri resourceUrl, int resourceIndex);
}
