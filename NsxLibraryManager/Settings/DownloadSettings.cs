namespace Nsxrenamer.Settings;

public class DownloadSettings
{
    public required string TitleDbPath { get; set; }
    public required string DownloadRegionUrl { get; set; }
    public bool Base { get; set; }
    public bool Dlc { get; set; }
    public bool Update { get; set; }
    public required IEnumerable<string> Regions { get; set; }
}