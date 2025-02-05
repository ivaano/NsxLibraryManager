using System.Collections.ObjectModel;
using NsxLibraryManager.Shared.Enums;

namespace NsxLibraryManager.Shared.Dto;

public record LibraryTitleDto
{
    public required string ApplicationId { get; init; }
    public string? BannerUrl { get; init; }
    public IEnumerable<CategoryDto>? Categories { get; init; }
    public CollectionDto? Collection { get; set; }
    public TitleContentType ContentType { get; init; }
    public string? Description { get; init; }
    public string? Developer { get; init; }
    public Collection<DlcDto>? Dlc { get; init; }
    public int? DlcCount { get; set; }
    public string? FileName { get; set; }
    public int Id { get; init; }
    public string? IconUrl { get; init; }
    public bool IsDemo { get; init; }
    public string? Intro { get; init; }
    public DateTime? LastWriteTime { get; init; }
    public IEnumerable<LanguageDto>? Languages { get; init; }
    public int? LatestOwnedUpdateVersion { get; init; }
    public int? LatestVersion { get; init; }
    public long? NsuId { get; init; }
    public int? NumberOfPlayers { get; init; }
    public string? OtherApplicationId { get; init; }
    public string? OtherApplicationName { get; set; }

    public Collection<DlcDto>? OwnedDlcs { get; init; }
    public int? OwnedDlcCount { get; init; }
    public Collection<UpdateDto>? OwnedUpdates { get; init; }
    public int? OwnedUpdatesCount { get; init; }
    public AccuratePackageType PackageType { get; init; }
    public int? PatchNumber { get; set; }
    public string? PatchTitleId { get; set; }
    public string? Publisher { get; init; }
    public int? Rating { get; init; }
    public IEnumerable<RatingContentDto>? RatingsContent { get; init; }
    public DateTime? ReleaseDate { get; init; }
    public string? Region { get; init; }
    public Collection<ScreenshotDto>? Screenshots { get; init; }
    public long Size { get; init; }
    public string? TitleName { get; set; }
    public uint TitleVersion { get; set; }
    public Collection<UpdateDto>? Updates { get; init; }
    public int? UpdatesCount { get; set; }
    public int? Version { get; init; }
    public Collection<VersionDto>? Versions { get; init; }
}