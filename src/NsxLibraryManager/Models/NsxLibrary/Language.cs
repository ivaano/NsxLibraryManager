using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NsxLibraryManager.Models.NsxLibrary;

public class Language : BaseLanguage
{
    public ICollection<Title> Titles { get; } = [];
}