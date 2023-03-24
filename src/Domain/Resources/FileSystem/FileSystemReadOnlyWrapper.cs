namespace Scrap.Domain.Resources.FileSystem;

public class FileSystemReadOnlyWrapper : IRawFileSystem
{
    private readonly IRawFileSystem _fileSystemImplementation;

    public FileSystemReadOnlyWrapper(IRawFileSystem fileSystemImplementation)
    {
        _fileSystemImplementation = fileSystemImplementation;
    }

    public Task DirectoryCreateAsync(string path) => Task.CompletedTask;

    public Task<bool> FileExistsAsync(string path) => _fileSystemImplementation.FileExistsAsync(path);

    public Task FileWriteAsync(string destinationPath, Stream stream) => Task.CompletedTask;

    public Task FileWriteAllTextAsync(string filePath, string content) => Task.CompletedTask;

    public Task<Stream> FileOpenReadAsync(string filePath) => _fileSystemImplementation.FileOpenReadAsync(filePath);

    public string PathCombine(string baseDirectory, string filePath) => _fileSystemImplementation.PathCombine(baseDirectory, filePath);

    public Task<string> FileReadAllTextAsync(string filePath) => _fileSystemImplementation.FileReadAllTextAsync(filePath);

    public string PathGetRelativePath(string relativeTo, string path) => _fileSystemImplementation.PathGetRelativePath(relativeTo, path);

    public string PathGetDirectoryName(string destinationPath) => _fileSystemImplementation.PathGetDirectoryName(destinationPath);

    public string PathNormalizeFolderSeparator(string path) => _fileSystemImplementation.PathNormalizeFolderSeparator(path);
    public bool IsReadOnly => true;
    public string DefaultGlobalUserConfigFile => _fileSystemImplementation.DefaultGlobalUserConfigFile;
    public Task<bool> DirectoryExistsAsync(string path) => _fileSystemImplementation.DirectoryExistsAsync(path);
}
