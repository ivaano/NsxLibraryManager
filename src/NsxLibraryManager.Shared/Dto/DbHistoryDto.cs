namespace NsxLibraryManager.Shared.Dto;

public record DbHistoryDto()
{
    public required string Version { get; init; }
    public required string Date { get; init; }
}