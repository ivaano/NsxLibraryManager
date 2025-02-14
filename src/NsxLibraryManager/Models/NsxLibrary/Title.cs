using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NsxLibraryManager.Shared.Enums;

namespace NsxLibraryManager.Models.NsxLibrary;


public sealed class Title : BaseTitle
{
    public int? OwnedUpdates { get; set; }
    public int? OwnedDlcs { get; set; }
    public int UserRating { get; set; }
    public bool Favorite {get; set;}
    
    public DateTime CreatedAt  { get; set; } 
    public DateTime UpdatedAt  { get; set; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(200)]
    public long? Notes { get; set; }
    
    //Title Version
    public uint Version { get; set; }
    
    //For basetitles and dlc only
    public uint LatestOwnedUpdateVersion { get; set; }

    public AccuratePackageType PackageType { get; set; }
    
    [Column(TypeName = "VARCHAR")]
    
    public required string FileName { get; set; }
    public DateTime? LastWriteTime { get; set; }
    
    public Collection? Collection { get; set; }

    public ICollection<Screenshot> Screenshots { get; set; } = [];
    public ICollection<Version> Versions { get; set; } = [];
    public ICollection<Category> Categories { get; set; } = [];
    public ICollection<TitleCategory> TitleCategories { get; set; } = [];
    public ICollection<Language> Languages { get; set; } = [];
    public ICollection<RatingsContent> RatingsContents { get; set; }  = [];

}