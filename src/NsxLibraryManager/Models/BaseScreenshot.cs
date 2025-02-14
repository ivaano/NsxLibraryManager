using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NsxLibraryManager.Models.NsxLibrary;

namespace NsxLibraryManager.Models;

public class BaseScreenshot
{
    [Required]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(200)]
    public required string Url { get; init; }
    public int? TitleId { get; init; }
    public int? EditionId { get; init; }

}