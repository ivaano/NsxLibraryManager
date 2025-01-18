using System.ComponentModel.DataAnnotations;
using NsxLibraryManager.Core.Enums;

namespace NsxLibraryManager.Core.Settings;

public class UserSettings
{
    [FileExtensions(Extensions = "db")]
    public string TitleDatabase { get; set; } = string.Empty;
    public string LibraryDatabase { get; set; } = string.Empty;
    public string LibraryPath { get; set; } = string.Empty;
    public bool Recursive { get; set; } = true;
    public string ProdKeys { get; set; } = string.Empty;

    public AgeRatingAgency AgeRatingAgency { get; set; } = AgeRatingAgency.Esrb;

    public required DownloadSettings DownloadSettings { get; set; }
}