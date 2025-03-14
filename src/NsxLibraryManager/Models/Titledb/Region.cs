using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NsxLibraryManager.Models.Titledb;


public sealed class Region : BaseRegion
{

    public ICollection<Language>? Languages { get; set; }
    public ICollection<Title> Titles { get; } = [];
}