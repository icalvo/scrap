using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal abstract class SearchOptions : OptionsBase
{
    protected SearchOptions(bool debug, bool verbose, string search) : base(debug, verbose)
    {
        Search = search;
    }

    [Description("Search with Regular Expression [bold][[pipeline]][/]")]
    [Value(0, Required = true, MetaName = "search")]
    public string Search { get; }
}
