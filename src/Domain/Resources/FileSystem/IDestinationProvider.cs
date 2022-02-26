using Scrap.Domain.Pages;

namespace Scrap.Domain.Resources.FileSystem;

public interface IDestinationProvider
{
    Task CompileAsync(FileSystemResourceRepositoryConfiguration config);
    Task<string> GetDestinationAsync(
        FileSystemResourceRepositoryConfiguration config,
        string rootFolder,
        IPage page,
        int pageIndex,
        Uri resourceUrl,
        int resourceIndex);
}
