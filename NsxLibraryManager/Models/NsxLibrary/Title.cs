using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NsxLibraryManager.Core.Enums;

namespace NsxLibraryManager.Models.NsxLibrary;


public sealed class Title : BaseTitle
{
    public int? OwnedUpdates { get; set; }
    public int? OwnedDlcs { get; set; }
    
    public int? Version { get; set; }

    public AccuratePackageType PackageType { get; set; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(200)]
    public required string FileName { get; set; }
    
    public ICollection<Edition>? Editions { get; init; }
    
    public DateTime? LastWriteTime { get; set; }
}