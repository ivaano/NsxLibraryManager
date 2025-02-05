using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NsxLibraryManager.Shared.Enums;

namespace NsxLibraryManager.Models.NsxLibrary;

public class Settings
{
    [Required]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }
    
    public required SettingsEnum Key { get; init; }
    
    [Column(TypeName = "TEXT")]
    public required string Value { get; set; }
}