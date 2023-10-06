using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Models;
using NsxLibraryManager.Services;

namespace NsxLibraryManager.Pages;

public partial class TitleDb
{
    [Inject]
    protected IDataService DataService { get; set; }
    
    private IEnumerable<RegionTitle> regionTitles;
    
    protected override async Task OnInitializedAsync()
    {
        regionTitles = await DataService.GetRegionTitlesAsync("US");
    }
}