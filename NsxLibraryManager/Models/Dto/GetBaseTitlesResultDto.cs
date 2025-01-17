namespace NsxLibraryManager.Models.Dto;

public record GetBaseTitlesResultDto
{
    public int Count { get; set; }
    public required IEnumerable<LibraryTitleDto> Titles { get; set; }
}