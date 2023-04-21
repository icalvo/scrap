using CommandLine;

namespace Scrap.CommandLine.Commands;

internal abstract class NameOrRootUrlOptions : OptionsBase
{
    [Value(0, HelpText = "Job definition name or root URL", MetaName = "Jobdef or root URL")]
    public string? NameOrRootUrl
    {
        get => Name ?? RootUrl;
        set
        {
            var isUri = Uri.TryCreate(value, UriKind.Absolute, out var uriResult) &&
                        (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (isUri)
            {
                Name = null;
                RootUrl = value;
            }
            else
            {
                Name = value;
                RootUrl = null;
            }
        }
    }

    [Option('n', "name", Required = false, HelpText = "Job definition name")]
    public string? Name { get; set; }

    [Option('r', "rooturl", Required = false, HelpText = "Root URL")]
    public string? RootUrl { get; set; }
}
