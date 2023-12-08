using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Models.Stats;
using NsxLibraryManager.Core.Services.Interface;

namespace NsxLibraryManager.Pages;

public partial class Index
{
    [Inject]
    protected IDataService DataService { get; set; } = default!;
    
    private CategoryDataItem[] _categories = Array.Empty<CategoryDataItem>();
    private PublisherDataItem[] _publishers = Array.Empty<PublisherDataItem>();
    private ContentDistributionItem[] _contentDistribution = Array.Empty<ContentDistributionItem>();
    private PackageDistributionItem[] _packageDistribution = Array.Empty<PackageDistributionItem>();
    
    private const bool ShowDataLabels = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    private Task LoadData()
    {
        var stats = DataService.GetLibraryTitlesStats();
        _categories = stats.CategoriesGames.Select(x => new CategoryDataItem { Category = x.Key, Count = x.Value }).Take(10).ToArray();
        _publishers = stats.PublisherGames.Select(x => new PublisherDataItem() { Publisher = x.Key, Count = x.Value }).Take(10).ToArray();
        var type = typeof(ContentDistribution);
        var properties = type.GetProperties();
        _contentDistribution = (from property in properties let value = property.GetValue(stats.ContentDistribution) let count = (int)value! let name = property.Name select new ContentDistributionItem { Type = name, Count = count }).OrderByDescending(x => x.Count).ToArray();
        type = typeof(PackageDistribution);
        properties = type.GetProperties();
        _packageDistribution = (from property in properties let value = property.GetValue(stats.PackageDistribution) let count = (int)value! let name = property.Name select new PackageDistributionItem { Type = name, Count = count }).OrderByDescending(x => x.Count).ToArray();
        
        return Task.CompletedTask;
    }
}

internal class PublisherDataItem
{
    public required string Publisher { get; set; }
    public int Count { get; set; }
    
}

internal class CategoryDataItem
{
    public required string Category { get; set; }
    public int Count { get; set; }
}

internal class ContentDistributionItem
{
    public required string Type { get; set; }
    public int Count { get; set; }
}

internal class PackageDistributionItem
{
    public required string Type { get; set; }
    public int Count { get; set; }
}