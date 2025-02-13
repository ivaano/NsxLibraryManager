using NsxLibraryManager.Shared.Dto;

namespace NsxLibraryManager.ViewModels.TitleDb;

public record GridPageViewModel
{
    public int TotalRecords { get; init; }
    public required IEnumerable<LibraryTitleDto> Titles { get; init; }
}