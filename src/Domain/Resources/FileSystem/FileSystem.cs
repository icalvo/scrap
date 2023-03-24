namespace Scrap.Domain.Resources.FileSystem;

public class FileSystem : IFileSystem
{
    private readonly IRawFileSystem _fileSystem;

    public FileSystem(IRawFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public PathTools Path => new(_fileSystem);
    public FileTools File => new(_fileSystem);
    public DirectoryTools Directory => new(_fileSystem);
    public bool IsReadOnly => _fileSystem.IsReadOnly;
    public string DefaultGlobalUserConfigFile => _fileSystem.DefaultGlobalUserConfigFile;
}
