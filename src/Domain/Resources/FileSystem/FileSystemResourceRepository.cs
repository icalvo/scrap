using Microsoft.Extensions.Logging;

namespace Scrap.Domain.Resources.FileSystem;

public class FileSystemResourceRepository : BaseResourceRepository<FileSystemResourceId>
{
    private readonly IDestinationProvider _destinationProvider;
    private readonly string _destinationRootFolder;
    private readonly bool _disableWrites;
    private readonly ILogger<FileSystemResourceRepository> _logger;
    private readonly IFileSystem _fileSystem;
    public FileSystemResourceRepository(
        IDestinationProvider destinationProvider,
        FileSystemResourceRepositoryConfiguration config,
        ILogger<FileSystemResourceRepository> logger,
        bool disableWrites,
        string? baseRootFolder,
        IFileSystem fileSystem)
    {
        _destinationProvider = destinationProvider;
        var rootFolder = fileSystem.PathNormalizeFolderSeparator(config.RootFolder);
        _destinationRootFolder =
            baseRootFolder != null ? fileSystem.PathCombine(baseRootFolder, rootFolder) : rootFolder;
        _logger = logger;
        _disableWrites = disableWrites;
        _fileSystem = fileSystem;
    }

    public override async Task<FileSystemResourceId> GetIdAsync(ResourceInfo resourceInfo)
    {
        var (page, pageIndex, resourceUrl, resourceIndex) = resourceInfo;
        var destinationPath = await _destinationProvider.GetDestinationAsync(
            _destinationRootFolder,
            page,
            pageIndex,
            resourceUrl,
            resourceIndex);
        var description = _fileSystem.PathGetRelativePath(_destinationRootFolder, destinationPath);

        return new FileSystemResourceId(destinationPath, description);
    }

    public override Task<bool> ExistsAsync(FileSystemResourceId id)
    {
        var destinationPath = id.FullPath;
        _logger.LogTrace("EXISTS {Destination}", destinationPath);
        return _fileSystem.FileExistsAsync(destinationPath);
    }

    public override async Task UpsertAsync(FileSystemResourceId id, Stream resourceStream)
    {
        var destinationPath = id.FullPath;
        var directoryName = _fileSystem.PathGetDirectoryName(destinationPath) ??
                            throw new InvalidOperationException(
                                $"Could not get directory name from destination path {destinationPath}");

        if (!_disableWrites)
        {
            _logger.LogTrace("WRITE {RelativePath}", destinationPath);
            await _fileSystem.FileWriteAsync(directoryName, destinationPath, resourceStream);
        }
        else
        {
            _logger.LogTrace(
                "FAKE. WRITE {RelativePath}",
                _fileSystem.PathGetRelativePath(_destinationRootFolder, destinationPath));
        }
    }
}
