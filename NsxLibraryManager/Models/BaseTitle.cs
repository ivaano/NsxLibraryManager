using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NsxLibraryManager.Core.Enums;

namespace NsxLibraryManager.Models;

public class BaseTitle
{
    [Required]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    [Column(TypeName = "VARCHAR")]
    [StringLength(200)]
    public long? NsuId { get; init; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(20)]
    public required string ApplicationId { get; init; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(20)]
    public string? OtherApplicationId { get; set; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(200)]
    public string? TitleName { get; init; }
    [Column(TypeName = "VARCHAR")]
    [StringLength(200)]
    public string? Intro { get; init; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(200)]
    public string? IconUrl { get; init; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(200)]
    public string? BannerUrl { get; init; }
    
    [Column(TypeName = "TEXT")]
    [StringLength(5000)]
    public string? Description { get; set; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(50)]
    public string? Developer { get; set; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(50)]
    public string? Publisher { get; set; }
    
    public DateTime? ReleaseDate { get; set; }
    
    public int? Rating { get; init; }
    public long? Size { get; init; }
    
    public int? NumberOfPlayers { get; init; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(2)]
    public string? Region { get; init; }
    
    public int? LatestVersion { get; set; }
    public int? UpdatesCount { get; set; }
    public int? DlcCount { get; set; }
    
    public bool IsDemo { get; set; }
    
    public TitleContentType ContentType { get; set; }


}