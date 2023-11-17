namespace NsxLibraryManager.Models.Dto;

public class Dlc
{
    public required string TitleId { get; init; }
    public string? TitleName { get; set; }
    public string TitleVersion { get; set; } = string.Empty;
    public bool Owned { get; set; } = false;
}