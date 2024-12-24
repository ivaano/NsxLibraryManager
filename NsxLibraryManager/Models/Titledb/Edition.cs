using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NsxLibraryManager.Models.Titledb;

public class Edition
{
    public int Id { get; init; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(20)]
    public required string ApplicationId { get; init; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(200)]
    public long? NsuId { get; init; }
  
    [Column(TypeName = "VARCHAR")]
    [StringLength(200)]
    public string? TitleName { get; init; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(200)]
    public string? BannerUrl { get; init; }
    
    [Column(TypeName = "TEXT")]
    [StringLength(5000)]
    public string? Description { get; set; }
   
    public DateTime? ReleaseDate { get; set; }
    
    public long? Size { get; init; }
    public int TitleId { get; set; }
    public ICollection<Screenshot>? Screenshots { get; set; }
    public Title Title { get; set; } = null!;
}