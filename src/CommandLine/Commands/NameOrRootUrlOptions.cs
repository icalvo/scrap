using CommandLine;

namespace Scrap.CommandLine.Commands;

internal abstract class NameOrRootUrlOptions : OptionsBase
{
    private readonly bool _isUri;

    protected NameOrRootUrlOptions(
        bool debug,
        bool verbose,
        string? nameOrRootUrlOption,
        string? nameOption,
        string? rootUrlOption) : base(debug, verbose)
    {
        NameOption = nameOption;
        RootUrlOption = rootUrlOption;
        _isUri = Uri.TryCreate(nameOrRootUrlOption, UriKind.Absolute, out var uriResult) &&
                 (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        NameOrRootUrlOption = nameOrRootUrlOption;

        if ((NameOption != null) & (nameOrRootUrlOption != null) & !_isUri)
        {
            throw new ArgumentException("Cannot provide a job def. name as value and as option", nameof(nameOption));
        }

        if ((RootUrlOption != null) & (nameOrRootUrlOption != null) & _isUri)
        {
            throw new ArgumentException("Cannot provide a root URL as value and as option", nameof(rootUrlOption));
        }
    }

    [Value(0, HelpText = "Job definition name or root URL", MetaName = "JOBDEF_OR_ROOT_URL")]
    public string? NameOrRootUrlOption { get; }

    [Option('n', "name", Required = false, HelpText = "Job definition name")]
    public string? NameOption { get; }

    [Option('r', "rooturl", Required = false, HelpText = "Root URL")]
    public string? RootUrlOption { get; }

    public string? Name
    {
        get
        {
            if (NameOption != null)
            {
                return NameOption;
            }

            if (!_isUri && NameOrRootUrlOption != null)
            {
                return NameOrRootUrlOption;
            }

            return null;
        }
    }

    public string? RootUrl
    {
        get
        {
            if (RootUrlOption != null)
            {
                return RootUrlOption;
            }

            if (_isUri && NameOrRootUrlOption != null)
            {
                return NameOrRootUrlOption;
            }

            return null;
        }
    }    
}
