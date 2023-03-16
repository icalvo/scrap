using Scrap.Domain.Resources.FileSystem;

namespace Scrap.Infrastructure.FileSystems;

public class LocalFileSystem : IFileSystem
{
    public Task<bool> FileExistsAsync(string path) => Task.FromResult(File.Exists(path));

    public async Task FileWriteAsync(string directoryName, string destinationPath, Stream stream)
    {
        Directory.CreateDirectory(directoryName);
        await using var outputStream = File.Open(destinationPath, FileMode.Create);
        await stream.CopyToAsync(outputStream);
    }

    public Task FileWriteAllTextAsync(string filePath, string content) =>
        File.WriteAllTextAsync(filePath, content);

    public Task<Stream> FileOpenReadAsync(string filePath) => Task.FromResult((Stream)File.OpenRead(filePath));
    public string PathCombine(string baseDirectory, string filePath) => Path.Combine(baseDirectory, filePath);

    public Task<string> FileReadAllTextAsync(string filePath) => File.ReadAllTextAsync(filePath);
    public string PathGetRelativePath(string relativeTo, string path) => Path.GetRelativePath(relativeTo, path);
    public string? PathGetDirectoryName(string destinationPath) => Path.GetDirectoryName(destinationPath);
    public string PathNormalizeFolderSeparator(string path) => path;
    public bool IsReadOnly => false;
}
