using System.Diagnostics.CodeAnalysis;

namespace Scrap.Domain.Resources.FileSystem;

[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Used when compiling expressions")]
public static class FileSystemExtensions
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used when compiling expressions")]
    public static string Combine(this PathTools pathTools, params string[] paths) =>
        paths.Aggregate(pathTools.Combine);
}
