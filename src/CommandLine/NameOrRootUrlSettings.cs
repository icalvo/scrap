using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Scrap.CommandLine;

internal abstract class NameOrRootUrlSettings : SettingsBase
{
    protected NameOrRootUrlSettings(bool debug, bool verbose, string nameOrRootUrl, string? name, string? rootUrl) : base(debug, verbose)
    {
        NameOrRootUrl = nameOrRootUrl;
        Name = name;
        RootUrl = rootUrl;
    }

    [Description("Job definition name")]
    [CommandArgument(0, "<nameOrRootUrl>")]
    public string NameOrRootUrl { get; }

    [Description("Job definition name")]
    [CommandOption("-n|--name")]
    public string? Name { get; private set; }

    [Description("URL where the scrapping starts")]
    [CommandOption("-r|--rootUrl")]
    public string? RootUrl { get; private set; }
    
    
    public override ValidationResult Validate()
    {
        bool isUri = Uri.TryCreate(NameOrRootUrl, UriKind.Absolute, out var uriResult)
                     && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        if (isUri)
        {
            if (RootUrl != null)
            {
                return ValidationResult.Error("Cannot have a Uri argument and --rootUrl at the same time");
            }

            RootUrl = NameOrRootUrl;
        }
        else
        {
            if (Name != null)
            {
                return ValidationResult.Error("Cannot have a Name argument and --name at the same time");
            }

            Name = NameOrRootUrl;
        }
        
        return ValidationResult.Success();
    }
}
