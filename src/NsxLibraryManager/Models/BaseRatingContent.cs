using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NsxLibraryManager.Models;


[PrimaryKey("Id")]
public class BaseRatingContent
{
    public int Id { get; set; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(30)]
    public required string Name { get; set; }
   
}