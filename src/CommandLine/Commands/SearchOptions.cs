using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal abstract class SearchOptions : OptionsBase
{
    [Description("Search with Regular Expression [bold][[pipeline]][/]")]
    [Value(0, MetaName = "search")]
    public string? search { get; }
}
