namespace NsxLibraryManager.Models;

public class GameVersions
{
    public string TitleId { get; set; }
    public string Version{ get; set; }
    public int VersionShifted{ get; set; }
    public string Date { get; set; }
    public bool Owned { get; set; } = false;
}