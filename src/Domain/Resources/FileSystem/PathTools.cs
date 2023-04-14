namespace Scrap.Domain.Resources.FileSystem;

public class PathTools
{
    private readonly IRawFileSystem _fileSystem;

    public PathTools(IRawFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public string Combine(string baseDirectory, string filePath) => _fileSystem.PathCombine(baseDirectory, filePath);
    public string GetRelativePath(string relativeTo, string path) => _fileSystem.PathGetRelativePath(relativeTo, path);
    public string GetDirectoryName(string destinationPath) => _fileSystem.PathGetDirectoryName(destinationPath);
    public string NormalizeFolderSeparator(string path) => _fileSystem.PathNormalizeFolderSeparator(path);

    public string ReplaceForbiddenChars(string path, string replacement = "") =>
        _fileSystem.PathReplaceForbiddenChars(path, replacement);
}
