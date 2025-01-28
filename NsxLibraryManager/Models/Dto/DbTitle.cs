using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Models.Titledb;

namespace NsxLibraryManager.Models.Dto;

public class DbTitle
{
    public int Id { get; set; }
    
    public long? NsuId { get; set; }
    public required string ApplicationId { get; init; }
    
    public string? OtherApplicationId { get; set; }
    
    public string? Intro { get; set; }
    
    public string? IconUrl { get; set; }
    
    public string? BannerUrl { get; set; }
    
    public string? Description { get; set; }
    
    public string? Developer { get; set; }
    
    public string? Publisher { get; set; }
    
    public DateTime? ReleaseDate { get; set; }
    
    public int? Rating { get; set; }
    public long? Size { get; set; }
    
    public int? NumberOfPlayers { get; set; }
    
    public string? Region { get; set; }
    
    public int? LatestVersion { get; set; }
    public int? UpdatesCount { get; set; }
    public int? DlcCount { get; set; }
    
    public bool IsDemo { get; set; }
    public string TitleName { get; set; }
   
    public int? Version { get; set; }
    
    public TitleContentType ContentType { get; set; }

    public AccuratePackageType PackageType { get; set; }
    
    public ICollection<Category> Categories { get; set; }
    public string CategoryNames => Categories is not null 
        ? string.Join(", ", Categories.Select(c => c.Name.Trim())) 
        : string.Empty;
}