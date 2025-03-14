namespace NsxLibraryManager.Shared.Dto;

public class RenameTitleDto
{
    public int Id { get; set; }
    public bool UpdateLibrary { get; set; } = false;
    public required string SourceFileName { get; set; }
    public string? DestinationFileName { get; set; }
    public string? TitleId { get; set; } 
    public string? TitleName { get; set; } 
    public bool RenamedSuccessfully { get; set; }
    public bool Error { get; set; }
    public string? ErrorMessage { get; set; }
}
