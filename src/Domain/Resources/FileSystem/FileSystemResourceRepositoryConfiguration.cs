using System.Diagnostics.CodeAnalysis;

namespace Scrap.Domain.Resources.FileSystem;

public class
    FileSystemResourceRepositoryConfiguration : BaseResourceRepositoryConfiguration<FileSystemResourceRepository>
{
    [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Deserialization by MemoryRepo")]
    public FileSystemResourceRepositoryConfiguration(string[] pathFragments, string rootFolder)
    {
        PathFragments = pathFragments;
        RootFolder = rootFolder;
    }

    public string RootFolder { get; } = null!;
    public string[] PathFragments { get; } = null!;

    public override string ToString() =>
        $"Folder: {RootFolder}\n{string.Join("\n", PathFragments.Select((exp, i) => $"Path Fragment {i + 1}: {exp}"))}";
}
