using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NsxLibraryManager.Models;

public sealed class Language
{
    public int Id { get; set; }
    [Column(TypeName = "VARCHAR")]
    [StringLength(2)]
    public string LanguageCode { get; set; }
    public ICollection<Region> Regions { get; } = [];
    public ICollection<Title> Titles { get; } = [];
}