namespace NsxLibraryManager.Models.NsxLibrary;

public sealed class Category : BaseCategory
{
    public ICollection<CategoryLanguage> Languages { get; set; } = null!;

    public ICollection<TitleCategory> TitleCategories { get; set; } = null!;

    public ICollection<Title> Titles { get; } = [];
}