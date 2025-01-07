using System.ComponentModel.DataAnnotations;

namespace NsxLibraryManager.Core.Settings;

public class UserSettings
{
    [FileExtensions(Extensions = "db")]
    public string TitleDatabase { get; set; } = string.Empty;
    public string LibraryPath { get; set; } = string.Empty;
    public bool Recursive { get; set; } = false;
    public string ProdKeys { get; set; } = string.Empty;

    public required DownloadSettings DownloadSettings { get; set; }
}