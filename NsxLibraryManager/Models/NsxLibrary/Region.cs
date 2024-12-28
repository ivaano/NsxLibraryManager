namespace NsxLibraryManager.Models.NsxLibrary;

public sealed class Region : BaseRegion
{

    public ICollection<Language>? Languages { get; set; }
    public ICollection<Title> Titles { get; } = [];
}