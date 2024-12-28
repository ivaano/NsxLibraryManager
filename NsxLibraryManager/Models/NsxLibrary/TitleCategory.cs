namespace NsxLibraryManager.Models.NsxLibrary;

public class TitleCategory : BaseTitleCategory
{
    public virtual Title Title { get; set; }
    public virtual Category Category { get; set; }
}