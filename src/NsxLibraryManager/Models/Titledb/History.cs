using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NsxLibraryManager.Models.Titledb;

[PrimaryKey("Id")]
public class History
{
    public int Id { get; set; }
    [Column(TypeName = "VARCHAR")]
    [StringLength(36)]
    public required string VersionNumber { get; set; }
    
    public required DateTime TimeStamp { get; set; }
    
    public int TitleCount { get; set; }
    public int BaseCount { get; set; }
    public int UpdateCount { get; set; }
    public int DlcCount { get; set; }

}