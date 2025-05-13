using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NsxLibraryManager.Models.NsxLibrary;

public class PersistentTitle
{
    [Required]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(20)]
    public required string ApplicationId { get; set; }
    
    public int UserRating { get; set; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(100)]
    public string? Collection { get; set; }
    
    public DateTime FirstSeen { get; set; }
}
