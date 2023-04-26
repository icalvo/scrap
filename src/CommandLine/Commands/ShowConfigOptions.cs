namespace Scrap.CommandLine.Commands;

internal sealed class ShowConfigOptions : OptionsBase
{
    public ShowConfigOptions(bool debug = false, bool verbose = false) : base(debug, verbose)
    {
    }

    public override bool ConsoleLog => false;
}
