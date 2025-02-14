using System.Collections.ObjectModel;

namespace NsxLibraryManager.Shared.Dto;

public record DlcDto()
{
    public required string ApplicationId { get; init; }
    public string? OtherApplicationId { get; init; }
    public string? TitleName { get; init; }
    public uint Version { get; init; }
    public long Size { get; init; }
    public bool Owned { get; set; }
    public string? FileName { get; init; }
    public Collection<ScreenshotDto>? Screenshots { get; init; }
};