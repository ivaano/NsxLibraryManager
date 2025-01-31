using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NsxLibraryManager.Models.NsxLibrary;

public class Collection
{
    [Required]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }
    
    [Column(TypeName = "VARCHAR")]
    public required string Name { get; init; }
    
    public virtual ICollection<Title> Titles { get; set; } = [];
}