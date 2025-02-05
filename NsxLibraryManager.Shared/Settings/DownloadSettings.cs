namespace NsxLibraryManager.Shared.Settings;

public class DownloadSettings
{
    public required int TimeoutInSeconds { get; set; }
    public required string TitleDbPath { get; set; }
    public required string TitleDbUrl { get; set; }
    public required string VersionUrl { get; set; }
}