using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Spectre.Console.Cli;

namespace Scrap.CommandLine;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class VersionCommand : Command
{
    public override int Execute(CommandContext context)
    {
        Console.WriteLine(GetVersion());

        return 0;
    }

    public static string? GetVersion() =>
        Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
}
