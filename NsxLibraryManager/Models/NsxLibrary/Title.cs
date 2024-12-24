using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NsxLibraryManager.Models.NsxLibrary;

[PrimaryKey("Id")]
public sealed class Title
{
    public int Id { get; init; }

    [Column(TypeName = "VARCHAR")]
    [StringLength(200)]
    public long? NsuId { get; init; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(20)]
    public required string ApplicationId { get; init; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(200)]
    public string? TitleName { get; init; }
}