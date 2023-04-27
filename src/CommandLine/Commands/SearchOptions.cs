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

    [Description("Search with Regular Expression [PIPELINE]")]
    [Value(0, Required = false, HelpText = "Regular expression to search", MetaName = "SEARCH")]
    public string? Search { get; }
}
