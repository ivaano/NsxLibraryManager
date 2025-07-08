using NsxLibraryManager.Shared.Dto;

namespace NsxLibraryManager.Contracts;

public class SearchResponse
{
    public int Total { get; set; }
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; }
    public required IEnumerable<LibraryTitleDto> Titles { get; set; }
}