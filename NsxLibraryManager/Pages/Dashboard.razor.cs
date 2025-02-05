using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Dto;

namespace NsxLibraryManager.Pages;

public partial class Dashboard
{
    [Inject]
    protected IStatsService StatsService { get; set; } = null!;
    
    private CategoryCountDto[] _categories = [];
    private PublisherCountDto[] _publishers = [];
    private ContentDistributionCountDto[] _contentDistribution = [];
    private PackageDistributionCountDto[] _packageDistribution = [];
    
    private const bool ShowDataLabels = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    private Task LoadData()
    {
        _categories = StatsService.GetTopCategories();
        _publishers = StatsService.GetTopPublishers();
        _contentDistribution = StatsService.GetTopContentDistribution();
        _packageDistribution = StatsService.GetTopPackageDistribution();
        return Task.CompletedTask;
    }
}
