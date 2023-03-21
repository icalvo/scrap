namespace Scrap.CommandLine;

public record GlobalConfig(string Key, string? DefaultValue, string Prompt, bool Optional = false);