using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NsxLibraryManager.Models;


public class BaseVersion
{
    [Required]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public uint VersionNumber { get; set; }
    public string VersionDate { get; set; }
    public int TitleId { get; set; }

}