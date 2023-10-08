using System.ComponentModel.DataAnnotations;
using Nsxrenamer.Settings;

namespace NsxLibraryManager.Settings;

public class AppSettings
{
    [Required]
    public required string TitleDatabase { get; set; } = string.Empty;
    [Required]
    public required string LibraryPath { get; set; } = string.Empty;
    public bool Recursive { get; set; } = false;
    public string ProdKeys { get; set; } = string.Empty;

    [Required]
    public required DownloadSettings DownloadSettings { get; set; }
}