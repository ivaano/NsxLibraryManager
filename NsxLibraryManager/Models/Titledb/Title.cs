namespace NsxLibraryManager.Models.Titledb;

public sealed class Title : BaseTitle
{
    public ICollection<Edition>? Editions { get; init; }
    public ICollection<Cnmt>? Cnmts { get; init; }
    public ICollection<Version>? Versions { get; init; }
    public ICollection<Language>? Languages { get; set; }
    public ICollection<Region>? Regions { get; set; }
    
    public ICollection<Screenshot>? Screenshots { get; set; }
    public ICollection<Category>? Categories { get; set; }
    public ICollection<RatingContent>? RatingContents { get; init; }
    
}