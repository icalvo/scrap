using System.Diagnostics.CodeAnalysis;

namespace Scrap.Domain.Resources.FileSystem;

public interface IFileSystem
{
    Task<bool> FileExistsAsync(string path);
    Task FileWriteAsync(string directoryName, string destinationPath, Stream stream);
    Task FileWriteAllTextAsync(string filePath, string content);
    Task<Stream> FileOpenReadAsync(string filePath);
    string PathCombine(string baseDirectory, string filePath);
    Task<string> FileReadAllTextAsync(string filePath);
    string PathGetRelativePath(string relativeTo, string path);
    string? PathGetDirectoryName(string destinationPath);
    string PathNormalizeFolderSeparator(string path);
}

[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Used when compiling expressions")]
public static class FileSystemExtensions
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used when compiling expressions")]
    public static string PathCombine(this IFileSystem fileSystem, params string[] paths) =>
        paths.Aggregate(fileSystem.PathCombine);
}
