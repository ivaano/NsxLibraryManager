using System.Collections.ObjectModel;
using NsxLibraryManager.Core.Enums;

namespace NsxLibraryManager.Models.Dto;

public record LibraryTitleDto
{
    public required string ApplicationId { get; init; }
    public string? OtherApplicationId { get; init; }
    public string? TitleName { get; init; }
    public long? NsuId { get; init; }
    public required string FileName { get; init; }
    public Collection<DlcDto>? Dlc { get; init; }
    public Collection<UpdateDto>? Updates { get; init; }
    public Collection<DlcDto>? OwnedDlcs { get; init; }
    public Collection<UpdateDto>? OwnedUpdates { get; init; }
    public IEnumerable<CategoryDto>? Categories { get; init; }
    public IEnumerable<RatingContentDto>? RatingsContent { get; init; }
    public Collection<ScreenshotDto>? Screenshots { get; init; }
    public Collection<VersionDto>? Versions { get; init; }
    public IEnumerable<LanguageDto>? Languages { get; init; }

    public AccuratePackageType PackageType { get; init; }
    public bool IsDemo { get; init; }
    public DateTime? LastWriteTime { get; init; }
    public required string ReleaseDate { get; init; }
    public int? DlcCount { get; init; }
    public int? OwnedDlcCount { get; init; }
    public int? LatestVersion { get; init; }
    public int? Version { get; init; }
    public int? NumberOfPlayers { get; init; }

    public int? Rating { get; init; }
    public int? UpdatesCount { get; init; }
    public required long Size { get; init; }
    public string? BannerUrl { get; init; }
    public string? Description { get; init; }
    public string? Developer { get; init; }
    public string? IconUrl { get; init; }

    public string? Intro { get; init; }

    public string? Publisher { get; init; }
    public string? Region { get; init; }

    public TitleContentType ContentType { get; init; }
}