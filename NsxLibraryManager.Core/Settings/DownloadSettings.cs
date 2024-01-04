namespace NsxLibraryManager.Core.Settings;

public class DownloadSettings
{
    public required int TimeoutInSeconds { get; set; }
    public required string TitleDbPath { get; set; }
    public required string RegionUrl { get; init; }
    public required string CnmtsUrl { get; init; }
    public required string VersionsUrl { get; init; }
    public bool Base { get; set; }
    public bool Dlc { get; set; }
    public bool Update { get; set; }
    public required IEnumerable<string> Regions { get; init; }
}