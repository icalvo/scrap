using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Scrap.Resources.FileSystem;

public class FileSystemResourceRepositoryConfiguration : IResourceRepositoryConfiguration
{
    [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Deserialization by Hangfire")]
    private FileSystemResourceRepositoryConfiguration()
    {
    }
        
    [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Deserialization by MemoryRepo")]
    public FileSystemResourceRepositoryConfiguration(string[] pathFragments, string rootFolder)
    {
        PathFragments = pathFragments;
        RootFolder = rootFolder;
    }

    public string Type => "filesystem";
    public string RootFolder { get; private set; } = null!;
    public string[] PathFragments { get; private set; } = null!;

    public void Validate(ILoggerFactory loggerFactory)
    {
        _ = CompiledDestinationProvider.CreateCompiled(
            PathFragments,
            new Logger<CompiledDestinationProvider>(loggerFactory));
    }

    public override string ToString()
    {
        return $"Folder: {RootFolder}\n" +
               string.Join("\n", PathFragments.Select((exp, i) => $"Path Fragment {i + 1}: {exp}"));
    }
}
