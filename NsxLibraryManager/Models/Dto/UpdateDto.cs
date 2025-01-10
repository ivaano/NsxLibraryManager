namespace NsxLibraryManager.Models.Dto;

public class UpdateDto
{
    public required string ApplicationId { get; init; }
    public string? OtherApplicationId { get; init; }
    public string? TitleName { get; init; }
    public string? FileName { get; init; }
    public int? Version { get; init; }
    public long? Size { get; init; }
}