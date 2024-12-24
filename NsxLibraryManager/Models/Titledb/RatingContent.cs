using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NsxLibraryManager.Models.Titledb;

[PrimaryKey("Id")]
public class RatingContent
{
    public int Id { get; set; }
    [Column(TypeName = "VARCHAR")]
    [StringLength(30)]
    public string Name { get; set; }
   
    public ICollection<Title> Titles { get; } = [];
}