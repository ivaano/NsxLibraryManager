namespace NsxLibraryManager.Models.NsxLibrary;

public class Category : BaseCategory
{
    public virtual ICollection<CategoryLanguage> Languages { get; set; }
    public ICollection<Title> Titles { get; } = [];
}