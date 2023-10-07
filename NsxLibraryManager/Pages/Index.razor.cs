using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Models;
using NsxLibraryManager.Pages.Components;
using NsxLibraryManager.Services;
using Radzen;

namespace NsxLibraryManager.Pages;

public partial class Index
{
    [Inject]
    protected IDataService DataService { get; set; }
    
    [Inject]
    protected ITitleLibraryService TitleLibraryService { get; set; }
    
    [Inject]
    protected DialogService DialogService { get; set; }
    
    private IEnumerable<LibraryTitle> libraryTitles;
    
    protected override async Task OnInitializedAsync()
    {
        libraryTitles = await DataService.GetLibraryTitlesAsync();
    }

    private async Task RefreshLibrary()
    {
        var confirmationResult = await DialogService.Confirm(
            "Are you sure?", "Refresh Library",
            new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" });
        if (confirmationResult is true)
        {
            DialogService.Close(confirmationResult);
            StateHasChanged();
            await DialogService.OpenAsync<ProgressDialog>("Refreshing library...");
        }
    }
}