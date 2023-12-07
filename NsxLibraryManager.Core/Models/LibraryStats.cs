namespace NsxLibraryManager.Core.Models;

public record LibraryStats
{
    public int TotalTitles { get; set; }

    public required Dictionary<string, int> CategoriesGames { get; set; }
    
    public ContentDistribution ContentDistribution { get; set; }
}
        
        