namespace NsxLibraryManager.Models.Dto;

public record LibraryTitleDto
{
    public long? NsuId { get; init; }
    public required string ApplicationId { get; init; }
    public string? OtherApplicationId { get; init; }
    public string? TitleName { get; init; }
    
}