using System.ComponentModel.DataAnnotations;

namespace Scrap.Common.Logging;

public class FileLoggingConfiguration
{
    [Required]
    [StringLength(2048, MinimumLength = 1)]
    public string? FilePath { get; set; }

    [Required]
    [StringLength(2048, MinimumLength = 1)]
    public string FolderPath { get; set; } = null!;
}
