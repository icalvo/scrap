namespace Scrap.Domain.Resources.FileSystem;

public class DirectoryTools
{
    private readonly IRawFileSystem _fileSystem;

    public DirectoryTools(IRawFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public async Task CreateIfNotExistAsync(string path)
    {
        if (!await _fileSystem.DirectoryExistsAsync(path))
        {
            await _fileSystem.DirectoryCreateAsync(path);
        }
    }

    public Task CreateAsync(string path) => _fileSystem.DirectoryCreateAsync(path);
    public Task<bool> ExistsAsync(string path) => _fileSystem.DirectoryExistsAsync(path);
}
