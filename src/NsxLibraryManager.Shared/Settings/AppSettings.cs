using System.ComponentModel.DataAnnotations;

namespace NsxLibraryManager.Shared.Settings;

public class AppSettings
{
    [Required, FileExtensions(Extensions = "db")]
    public required string TitledbDbConnection { get; set; } = string.Empty;
    [Required, FileExtensions(Extensions = "db")]
    public required string NsxLibraryDbConnection { get; set; } = string.Empty;
    [Required]
    public required string SqlTitleDbRepository { get; set; } = string.Empty;
    
    public bool DownloadTitleDbOnStartup { get; set; } = true;
}