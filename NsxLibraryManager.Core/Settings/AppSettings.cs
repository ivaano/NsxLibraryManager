using System.ComponentModel.DataAnnotations;

namespace NsxLibraryManager.Core.Settings;

public class AppSettings
{
    [Required]
    public required string TitleDatabase { get; init; } = string.Empty;
    [Required]
    public required string LibraryPath { get; init; } = string.Empty;
    public bool Recursive { get; set; } = false;
    public string ProdKeys { get; init; } = string.Empty;

    [Required]
    public required DownloadSettings DownloadSettings { get; init; }
}