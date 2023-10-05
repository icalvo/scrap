using System.Diagnostics.CodeAnalysis;
using CommandLine.Text;

namespace Scrap.CommandLine.Commands;

internal sealed class ShowConfigOptions : OptionsBase
{
    public ShowConfigOptions(bool debug = false, bool verbose = false) : base(debug, verbose)
    {
    }

    public override bool ConsoleLog => false;


    [Usage]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Called by CommandLineParser")]
    public static IEnumerable<Example> Examples =>
        new[] { new Example("Shows all configuration values", new ShowConfigOptions()) };    
}
