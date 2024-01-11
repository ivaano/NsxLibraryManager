namespace NsxLibraryManager.Core.Settings;

public class DownloadSettings
{
    public required int TimeoutInSeconds { get; set; }
    public required string TitleDbPath { get; set; }
    public required string RegionUrl { get; set; }
    public required string CnmtsUrl { get; set; }
    public required string VersionsUrl { get; set; }
    public bool Base { get; set; }
    public bool Dlc { get; set; }
    public bool Update { get; set; }
    public required IEnumerable<string> Regions { get; init; }
}