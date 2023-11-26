namespace NsxLibraryManager.Core.Models;

public class GameVersions
{
    public string TitleId { get; set; } = string.Empty;
    public string ApplicationId { get; set; } = string.Empty;
    public string Version{ get; set; } = string.Empty;
    public int VersionShifted{ get; set; }
    public string Date { get; set; } = string.Empty;
    public bool Owned { get; set; } = false;
}