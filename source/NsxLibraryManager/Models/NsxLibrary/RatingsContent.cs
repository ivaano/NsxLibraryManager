namespace NsxLibraryManager.Models.NsxLibrary;

public class RatingsContent : BaseRatingContent
{
    public ICollection<Title> Titles { get; } = [];
}