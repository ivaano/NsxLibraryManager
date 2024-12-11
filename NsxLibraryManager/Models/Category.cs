using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NsxLibraryManager.Core.Models;

namespace NsxLibraryManager.Models;

[PrimaryKey("Id")]
public class Category
{
    public int Id { get; set; }
    [Column(TypeName = "VARCHAR")]
    [StringLength(30)]
    public string Name { get; set; }
    
    public virtual ICollection<CategoryLanguage> Languages { get; set; }
    public ICollection<Title> Titles { get; } = [];
}