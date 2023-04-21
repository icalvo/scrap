using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class VersionCommand
{
    public int Execute()
    {
        Console.WriteLine(GetVersion());

        return 0;
    }

    public static string? GetVersion() =>
        Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
}
