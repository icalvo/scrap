namespace Scrap.Domain.Resources.FileSystem;

public interface IRawFileSystem
{
    Task DirectoryCreateAsync(string path);
    Task<bool> FileExistsAsync(string path);
    Task FileWriteAsync(string destinationPath, Stream stream);
    Task FileWriteAllTextAsync(string filePath, string content);
    Task<Stream> FileOpenReadAsync(string filePath);
    Task<string> FileReadAllTextAsync(string filePath);
    string PathCombine(string baseDirectory, string filePath);
    string PathGetRelativePath(string relativeTo, string path);
    string PathGetDirectoryName(string destinationPath);
    string PathNormalizeFolderSeparator(string path);
    bool IsReadOnly { get; }
    string DefaultGlobalUserConfigFile { get; }
    Task<bool> DirectoryExistsAsync(string path);
    string PathReplaceForbiddenChars(string path, string replacement = "");
}
