using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NsxLibraryManager.Core.Models;

namespace NsxLibraryManager.Models;

[PrimaryKey("Id")]
public class Region
{
    public int Id { get; set; }
    [Column(TypeName = "VARCHAR")]
    [StringLength(2)]
    public string Name { get; set; }
    public virtual ICollection<Language>? Languages { get; set; }
    public ICollection<Title> Titles { get; } = [];
}