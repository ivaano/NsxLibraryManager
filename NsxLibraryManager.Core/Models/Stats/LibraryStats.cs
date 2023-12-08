namespace NsxLibraryManager.Core.Models.Stats;

public record LibraryStats
{
    public int TotalTitles { get; set; }

    public required Dictionary<string, int> CategoriesGames { get; set; }
    public required Dictionary<string, int> PublisherGames { get; set; }
    
    public required ContentDistribution ContentDistribution { get; set; }
    
    public required PackageDistribution PackageDistribution { get; set; }
}
        
        