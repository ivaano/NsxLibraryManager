namespace NsxLibraryManager.Models.NsxLibrary;

public class Edition : BaseEdition
{
    public ICollection<Screenshot>? Screenshots { get; set; }
    public Title Title { get; set; } = null!;
}