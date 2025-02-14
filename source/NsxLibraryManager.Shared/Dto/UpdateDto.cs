namespace NsxLibraryManager.Shared.Dto;

public class UpdateDto
{
    public required string ApplicationId { get; init; }
    public string? OtherApplicationId { get; init; }
    public string? TitleName { get; init; }
    public string? FileName { get; init; }
    public DateOnly? Date { get; init; }
    public uint Version { get; init; }
    public int? ShortVersion { get; init; }
    public long Size { get; init; }
    public bool Owned { get; init; }
}