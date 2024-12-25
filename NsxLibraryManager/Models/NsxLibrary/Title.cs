namespace NsxLibraryManager.Models.NsxLibrary;


public sealed class Title : BaseTitle
{
    public ICollection<Edition>? Editions { get; init; }

}