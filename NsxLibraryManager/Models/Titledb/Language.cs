using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NsxLibraryManager.Models.Titledb;

public sealed class Language : BaseLanguage
{

    public ICollection<Region> Regions { get; } = [];
    public ICollection<Title> Titles { get; } = [];
}