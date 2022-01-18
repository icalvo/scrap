using System.ComponentModel.DataAnnotations;

namespace Scrap.CommandLine.Logging;

public class FileLoggingConfiguration
{
    [Required]
    [StringLength(2048, MinimumLength = 1)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [StringLength(2048, MinimumLength = 1)]
    public string FolderPath { get; set; } = string.Empty;
}
