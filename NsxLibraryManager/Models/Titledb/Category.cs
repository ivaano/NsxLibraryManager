using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NsxLibraryManager.Models.Titledb;

public sealed class Category : BaseCategory
{
    public ICollection<CategoryLanguage> Languages { get; set; }
    public ICollection<Title> Titles { get; } = [];
}