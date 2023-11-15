using System.ComponentModel.DataAnnotations;
using NsxLibraryManager.Enums;
using NsxLibraryManager.FileLoading.QuickFileInfoLoading;

namespace NsxLibraryManager.Models;

public record LibraryTitle
{
    public int Id { get; set; }
    public required string TitleId { get; init; }
    public long Nsuid { get; set; }
    public int? NumberOfPlayers { get; set; }
    // Last Patch Version in TitleDb
    public uint AvailableVersion { get; set; }
    public string? ApplicationTitleId { get; set; }
    public string? ApplicationTitleName { get; set; }
    public string? PatchTitleId { get; set; }
    [DisplayFormat(DataFormatString = @"{0:d MM\ddd\yyyy}")]
    public DateTime ReleaseDate { get; set; }
    public int? PatchNumber { get; set; }
    public string? TitleName { get; set; }
    public string? Publisher { get; set; }
    public uint TitleVersion { get; init; }
    public TitleLibraryType Type { get; set; }
    public AccuratePackageType PackageType { get; init; }
    public required string FileName { get; set; }
    public string? BannerUrl { get; set; }
    public List<string>? Category { get; set; }
    public string? Description { get; set; }
    public string? Developer { get; set; }
    public string? FrontBoxArt { get; set; }
    public string? IconUrl { get; set; }
    public string? Intro { get; set; }
    public List<string>? Screenshots { get; set; }
    public List<string>? AvailableDlcs { get; set; }
    public List<string>? OwnedDlcs { get; set; }
    public List<int>? OwnedUpdates { get; set; }
    public int? Rating { get; set; }
    public List<string>? RatingContent { get; set; }
    public long? Size { get; set; }
    public List<string>? Languages { get; set; }

    public string? NewFileName { get; set; }
    public bool Error { get; set; } = false;
    public string ErrorMessage { get; set; } = string.Empty;
    [DisplayFormat(DataFormatString = @"{0:d MM\ddd\yyyy}")]
    public DateTime LastUpdated { get; set; }
}