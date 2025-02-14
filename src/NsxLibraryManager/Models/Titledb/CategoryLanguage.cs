using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NsxLibraryManager.Models.Titledb;

public class CategoryLanguage : BaseCategoryLanguage
{
    public Category Category { get; set; }
}