namespace Scrap.Domain.Resources.FileSystem;

public interface IFileSystem
{
    public PathTools Path { get; }
    public FileTools File { get; }
    public DirectoryTools Directory { get; }
    bool IsReadOnly { get; }
}
