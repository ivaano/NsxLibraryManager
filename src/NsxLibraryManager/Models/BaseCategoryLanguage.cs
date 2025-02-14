using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NsxLibraryManager.Models;

[PrimaryKey("Id")]
public class BaseCategoryLanguage
{
    [Required]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(2)]
    
    public string Region { get; set; }
    [Column(TypeName = "VARCHAR")]
    [StringLength(2)]
    
    public string Language { get; set; }
    [Column(TypeName = "VARCHAR")]
    [StringLength(30)]
    
    public string Name { get; set; }
    public int CategoryId { get; set; }
   
}