using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NsxLibraryManager.Models.Titledb;

[PrimaryKey("Id")]
public sealed class Screenshot
{
    public int Id { get; init; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(200)]
    public string? Url { get; init; }
    public int? TitleId { get; init; }
    public int? EditionId { get; init; }
    public Title? Title { get; init; }
    public Edition? Edition { get; init; }
}