namespace NsxLibraryManager.Models.Titledb;


public class RatingContent : BaseRatingContent
{
    public ICollection<Title> Titles { get; } = [];
}