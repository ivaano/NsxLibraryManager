using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NsxLibraryManager.Models.Titledb;

[PrimaryKey("Id")]
public class NswReleaseTitle
{
    public int Id { get; init; }

    [Column(TypeName = "VARCHAR")]
    [StringLength(20)]
    public required string ApplicationId { get; init; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(200)]
    public required string TitleName { get; init; }
   
    [Column(TypeName = "VARCHAR")]
    [StringLength(200)]
    public string? Revision { get; init; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(50)]
    public string? Publisher { get; set; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(2)]
    public string? Region { get; init; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(200)]
    public string? Languages { get; init; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(50)]
    public string? Firmware { get; set; }
    
    public uint Version { get; set; }

}