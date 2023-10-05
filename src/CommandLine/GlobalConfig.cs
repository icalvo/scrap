namespace Scrap.CommandLine;

public record GlobalConfig(string[] Keys, string? DefaultValue, string Prompt, bool Optional = false)
{
    public GlobalConfig(string key, string? DefaultValue, string Prompt, bool Optional = false) : this(
        new[] { key },
        DefaultValue,
        Prompt,
        Optional)
    {
    }
}
