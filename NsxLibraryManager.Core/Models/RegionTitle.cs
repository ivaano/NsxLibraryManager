using NsxLibraryManager.Core.Enums;

namespace NsxLibraryManager.Core.Models;

public class RegionTitle
{
    public long Id { get; set; }
    public string? TitleId { get; set; }
    public List<string>? Ids { get; set; }
    public string? BannerUrl { get; set; }
    public List<string>? Category { get; set; }
    public string? Categories { get; set; }
    public string? Description { get; set; }
    public string? Developer { get; set; }
    public string? FrontBoxArt { get; set; }
    public string? IconUrl { get; set; }
    public string? Intro { get; set; }
    public bool IsDemo { get; set; }
    public string? Key { get; set; }
    public TitleLibraryType Type { get; set; }
    public List<string>? Languages { get; set; }
    public string? Name { get; set; }
    public int? NumberOfPlayers { get; set; }
    public string? Publisher { get; set; }
    public int? Rating { get; set; }
    public List<string>? RatingContent { get; set; }
    public string? Region { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string? RightsId { get; set; }
    public List<string>? Screenshots { get; set; }
    public long? Size { get; set; }
    public string? Version { get; set; }
    public DateTime CreatedTime { get; set; }
}