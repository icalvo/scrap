namespace Scrap.Domain.Resources.FileSystem;

public class FileSystemResourceId : IResourceId
{
    public FileSystemResourceId(string fullPath, string relativePath)
    {
        FullPath = fullPath;
        RelativePath = relativePath;
    }

    public string FullPath { get; }
    public string RelativePath { get; }

    public string ToText() => FullPath;

    public override string ToString() => ToText();
}
