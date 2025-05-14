using System.ComponentModel.DataAnnotations;
using NsxLibraryManager.Shared.Enums;

namespace NsxLibraryManager.Shared.Settings;

public class UserSettings
{
    [FileExtensions(Extensions = "db")]
    public string TitleDatabase { get; set; } = string.Empty;
    public string LibraryDatabase { get; set; } = string.Empty;
    public List<LibraryLocation> LibraryLocations { get; set; } = [];
    public string BackupPath { get; set; } = string.Empty;
    public bool Recursive { get; set; } = true;
    public string ProdKeys { get; set; } = string.Empty;
    public string TitleKeys { get; set; } = string.Empty;
    public string ConsoleKeys { get; set; } = string.Empty;
    public string UiTheme { get; set; } = string.Empty;
    public bool IsConfigured { get; set; }
    public bool LibraryReloadPostWebhook { get; set; }
    public string LibraryReloadWebhookUrl { get; set; } = string.Empty;
    public int LibraryReloadWebhookRequestMethod { get; set; } = 1;
    public bool LibraryRefreshPostWebhook { get; set; }
    public string LibraryRefreshWebhookUrl { get; set; } = string.Empty;
    public int LibraryRefreshWebhookRequestMethod { get; set; } = 1;

    public bool UseEnglishNaming { get; set; } = true;
    public AgeRatingAgency AgeRatingAgency { get; set; } = AgeRatingAgency.Esrb;

    public required DownloadSettings DownloadSettings { get; set; }
}