namespace NsxLibraryManager.Shared.Dto;

public record VersionDto
{
    public int VersionNumber { get; init; }
    public int ShortVersionNumber { get; init; }
    public DateOnly VersionDate { get; init; }
}