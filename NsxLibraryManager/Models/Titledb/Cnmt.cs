using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NsxLibraryManager.Models.Titledb;

public class Cnmt
{
    public int Id { get; set; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(20)]
    public string? OtherApplicationId { get; set; }
    
    public int? RequiredApplicationVersion { get; set; }
    public int TitleType { get; set; }
    public int TitleId { get; set; }
    public int Version { get; set; }
    public Title Title { get; set; }
}