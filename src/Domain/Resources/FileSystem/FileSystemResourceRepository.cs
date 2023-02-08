using Microsoft.Extensions.Logging;

namespace Scrap.Domain.Resources.FileSystem;

public class FileSystemResourceRepository : BaseResourceRepository<FileSystemResourceId>
{
    private readonly IDestinationProvider _destinationProvider;
    private readonly string _destinationRootFolder;
    private readonly bool _disableWrites;
    private readonly ILogger<FileSystemResourceRepository> _logger;

    public FileSystemResourceRepository(
        IDestinationProvider destinationProvider,
        FileSystemResourceRepositoryConfiguration config,
        ILogger<FileSystemResourceRepository> logger,
        bool disableWrites)
    {
        _destinationProvider = destinationProvider;
        _destinationRootFolder = config.RootFolder;
        _logger = logger;
        _disableWrites = disableWrites;
    }

    public override async Task<FileSystemResourceId> GetIdAsync(ResourceInfo resourceInfo)
    {
        var (page, pageIndex, resourceUrl, resourceIndex) = resourceInfo;
        var destinationPath = await _destinationProvider.GetDestinationAsync(
            _destinationRootFolder,
            page, pageIndex,
            resourceUrl, resourceIndex);
        var description = Path.GetRelativePath(_destinationRootFolder, destinationPath);

        return new FileSystemResourceId(destinationPath, description);
    }

    public override Task<bool> ExistsAsync(FileSystemResourceId id)
    {
        var destinationPath = id.FullPath;
        return Task.FromResult(File.Exists(destinationPath));
    }

    public override async Task UpsertAsync(FileSystemResourceId id, Stream resourceStream)
    {
        var destinationPath = id.FullPath;
        var directoryName = Path.GetDirectoryName(destinationPath)
                            ?? throw new InvalidOperationException(
                                $"Could not get directory name from destination path {destinationPath}");

        if (!_disableWrites)
        {
            _logger.LogTrace("WRITE {RelativePath}", Path.GetRelativePath(_destinationRootFolder, destinationPath));
            Directory.CreateDirectory(directoryName);
            await using var outputStream = File.Open(destinationPath, FileMode.Create);
            await resourceStream.CopyToAsync(outputStream);
        }
        else
        {
            _logger.LogTrace("FAKE. WRITE {RelativePath}",
                Path.GetRelativePath(_destinationRootFolder, destinationPath));
        }
    }
}
