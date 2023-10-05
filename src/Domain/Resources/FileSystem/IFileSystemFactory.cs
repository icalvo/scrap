namespace Scrap.Domain.Resources.FileSystem;

public interface IFileSystemFactory
{
    public Task<IFileSystem> BuildAsync(bool? readOnly = false);
    string FileSystemType { get; }
}
