namespace Scrap.Domain.Resources.FileSystem;

public interface IFileSystem
{
    PathTools Path { get; }
    FileTools File { get; }
    DirectoryTools Directory { get; }
    bool IsReadOnly { get; }
    public string DefaultGlobalUserConfigFile { get; }
 
}
