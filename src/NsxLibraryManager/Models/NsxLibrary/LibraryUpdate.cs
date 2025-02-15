using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NsxLibraryManager.Models.NsxLibrary;

public class LibraryUpdate
{
    [Required]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }
    
    public DateTime DateCreated { get; init; }
    public DateTime DateUpdated { get; init; }
    public int BaseTitleCount { get; init; }
    public int UpdateTitleCount { get; init; }
    public int DlcTitleCount { get; init; }
    
    [StringLength(2000)]
    public required string LibraryPath { get; init; }
}