namespace NsxLibraryManager.Models.NsxLibrary;

public class Language : BaseLanguage
{
    
    public ICollection<Region> Regions { get; } = [];
    public ICollection<Title> Titles { get; } = [];
}