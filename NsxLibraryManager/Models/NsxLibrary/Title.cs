using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NsxLibraryManager.Core.Enums;

namespace NsxLibraryManager.Models.NsxLibrary;


public sealed class Title : BaseTitle
{
    public int? OwnedUpdates { get; set; }
    public int? OwnedDlcs { get; set; }
    
    //Title Version
    public int? Version { get; set; }
    
    //For basetitles and dlc only
    public int? LatestOwnedUpdateVersion { get; set; }

    public AccuratePackageType PackageType { get; set; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(200)]
    public required string FileName { get; set; }
    public DateTime? LastWriteTime { get; set; }
   
    public ICollection<Screenshot>? Screenshots { get; set; }
    public ICollection<Version>? Versions { get; set; }
    public ICollection<Category>? Categories { get; set; }
    public ICollection<TitleCategory> TitleCategories { get; set; }
    public ICollection<Language> Languages { get; set; } = [];
    
    public ICollection<RatingsContent> RatingsContents { get; set; }  = [];

}