using Microsoft.Extensions.Logging;

namespace Scrap.Domain.Resources.FileSystem;

public class FileSystemResourceRepository : BaseResourceRepository<FileSystemResourceId>
{
    private readonly IDestinationProvider _destinationProvider;
    private readonly string _destinationRootFolder;
    private readonly ILogger<FileSystemResourceRepository> _logger;
    private readonly IFileSystem _fileSystem;

    private FileSystemResourceRepository(
        IDestinationProvider destinationProvider,
        FileSystemResourceRepositoryConfiguration config,
        ILogger<FileSystemResourceRepository> logger,
        string? baseRootFolder,
        IFileSystem fileSystem)
    {
        _destinationProvider = destinationProvider;
        var rootFolder = fileSystem.Path.NormalizeFolderSeparator(config.RootFolder);
        _destinationRootFolder =
            baseRootFolder != null ? fileSystem.Path.Combine(baseRootFolder, rootFolder) : rootFolder;
        _logger = logger;
        _fileSystem = fileSystem;
        
        _logger.LogInformation("Resource Repo Base Folder: {BaseFolder}", _destinationRootFolder);
    }

    public static async Task<FileSystemResourceRepository> BuildAsync(
        IDestinationProvider destinationProvider,
        FileSystemResourceRepositoryConfiguration config,
        ILogger<FileSystemResourceRepository> logger,
        string? baseRootFolder,
        IFileSystemFactory fileSystemFactory)
    {
        return new FileSystemResourceRepository(
            destinationProvider,
            config,
            logger,
            baseRootFolder,
            await fileSystemFactory.BuildAsync(true));
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
        var description = _fileSystem.Path.GetRelativePath(_destinationRootFolder, destinationPath);

        return new FileSystemResourceId(destinationPath, description);
    }

    public override Task<bool> ExistsAsync(FileSystemResourceId id)
    {
        var destinationPath = id.FullPath;
        return _fileSystem.File.ExistsAsync(destinationPath);
    }

    public override async Task UpsertAsync(FileSystemResourceId id, Stream resourceStream)
    {
        var destinationPath = id.FullPath;
        var directoryName = _fileSystem.Path.GetDirectoryName(destinationPath) ??
                            throw new InvalidOperationException(
                                $"Could not get directory name from destination path {destinationPath}");

        var relativePath = _fileSystem.Path.GetRelativePath(_destinationRootFolder, destinationPath);
        if (_fileSystem.IsReadOnly)
        {
            _logger.LogTrace("FAKE. WRITE {RelativePath}", relativePath);
        }
        else
        {
            _logger.LogTrace("WRITE {RelativePath}", relativePath);
            await _fileSystem.Directory.CreateIfNotExistAsync(directoryName);
            await _fileSystem.File.WriteAsync(destinationPath, resourceStream);
        }
    }
}
