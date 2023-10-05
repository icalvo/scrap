namespace Scrap.Domain.Resources.FileSystem;

public class FileTools
{
    private readonly IRawFileSystem _fileSystem;

    public FileTools(IRawFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public Task<bool> ExistsAsync(string path) => _fileSystem.FileExistsAsync(path);
    public Task WriteAsync(string destinationPath, Stream stream) => _fileSystem.FileWriteAsync(destinationPath, stream);
    public Task WriteAllTextAsync(string filePath, string content) => _fileSystem.FileWriteAllTextAsync(filePath, content);
    public Task<Stream> OpenReadAsync(string filePath) => _fileSystem.FileOpenReadAsync(filePath);
    public Task<string> ReadAllTextAsync(string filePath) => _fileSystem.FileReadAllTextAsync(filePath);
}