using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Models;
using NsxLibraryManager.Pages.Components;
using NsxLibraryManager.Services;
using Radzen;

namespace NsxLibraryManager.Pages;

public partial class TitleDb
{
    [Inject]
    protected IDataService DataService { get; set; }
  
    [Inject]
    protected DialogService DialogService { get; set; }
    
    private IEnumerable<RegionTitle> regionTitles;
    
    protected override async Task OnInitializedAsync()
    {
        regionTitles = await DataService.GetRegionTitlesAsync("US");
    }

    private async Task RefreshTitleDb()
    {
        var confirmationResult = await DialogService.Confirm(
                "Are you sure?", "Refresh TitleDb",
                new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" });
        if (confirmationResult is true)
        {
            DialogService.Close();
            await DialogService.OpenAsync<RefreshTitleDbProgressDialog>("Refreshing titledb...");
            StateHasChanged();
        }
    }
}