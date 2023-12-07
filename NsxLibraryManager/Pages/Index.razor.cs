using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Services.Interface;

namespace NsxLibraryManager.Pages;

public partial class Index
{
    [Inject]
    protected IDataService DataService { get; set; } = default!;
    
    private CategoryDataItem[] _categories = Array.Empty<CategoryDataItem>();
    private ContentDistributionItem[] _contentDistribution = Array.Empty<ContentDistributionItem>();
    
    private const bool ShowDataLabels = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        var stats = DataService.GetLibraryTitlesStats();
        _categories = stats.CategoriesGames.Select(x => new CategoryDataItem { Category = x.Key, Count = x.Value }).Take(10).ToArray();
        var type = typeof(ContentDistribution);
        var properties = type.GetProperties();
        _contentDistribution = (from property in properties let value = property.GetValue(stats.ContentDistribution) let count = (int)value! let name = property.Name select new ContentDistributionItem { Type = name, Count = count }).ToArray();

    }
}

internal class CategoryDataItem
{
    public string Category { get; set; }
    public int Count { get; set; }
}

internal class ContentDistributionItem
{
    public string Type { get; set; }
    public int Count { get; set; }
}