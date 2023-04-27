﻿using System.Diagnostics.CodeAnalysis;
using CommandLine;
using CommandLine.Text;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Verb("searchvisited", aliases: new[] { "sv" }, HelpText = "Searches visited pages")]
internal sealed class SearchVisitedOptions : SearchOptions
{
    public SearchVisitedOptions(string search, bool debug = false, bool verbose = false) : base(debug, verbose, search)
    {
    }

    public override bool ConsoleLog => false;

    [Usage]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Called by CommandLineParser")]
    public static IEnumerable<Example> Examples =>
        new[]
        {
            new Example(
                "Searches all visited pages containing 'example.com'",
                new UnParserSettings { HideDefaultVerb = true },
                new SearchVisitedOptions(".*example\\.com.*"))
        };    
}
