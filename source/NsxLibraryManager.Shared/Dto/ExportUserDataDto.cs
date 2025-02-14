namespace NsxLibraryManager.Shared.Dto;

public class ExportUserDataDto
{
    public required string ApplicationId { get; set; }
    public int UserRating { get; set; }
    public string? Collection { get; set; }
}