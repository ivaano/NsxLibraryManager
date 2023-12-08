namespace NsxLibraryManager.Core.Models.Stats;

public record ContentDistribution
{
    public int Base { get; set; }
    public int Dlcs { get; set; }
    public int Updates { get; set; }
}