using LibHac.Ncm;
using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Models;
using NsxLibraryManager.Pages.Components;
using NsxLibraryManager.Services;
using Radzen;
using Radzen.Blazor;

namespace NsxLibraryManager.Pages;
#nullable disable
public partial class Index
{
    [Inject]
    protected IDataService DataService { get; set; }
    
    [Inject]
    protected ITitleLibraryService TitleLibraryService { get; set; }
   
    [Inject]
    protected DialogService DialogService { get; set; }
    
    public IEnumerable<LibraryTitle> LibraryTitles;
    public RadzenDataGrid<LibraryTitle> Grid;
    public int AppCount = 0;
    public int PatchCount = 0;
    public int DlcCount = 0;
    public string LibraryPath = string.Empty;
    
    protected override async Task OnInitializedAsync()
    {
        LibraryTitles = await DataService.GetLibraryTitlesAsync();
        LibraryPath = TitleLibraryService.GetLibraryPath();
        CalculateCounts();
    }
    
    private void CalculateCounts()
    {
        var libTitleList = LibraryTitles.ToList();
        
        AppCount = libTitleList.Count(x => x.Type == ContentMetaType.Application);
        PatchCount = libTitleList.Count(x => x.Type == ContentMetaType.Patch);
        DlcCount = libTitleList.Count(x => x.Type == ContentMetaType.AddOnContent);
    }

    private async Task RefreshLibrary()
    {
        var confirmationResult = await DialogService.Confirm(
            "Are you sure?", "Refresh Library",
            new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" });
        if (confirmationResult is true)
        {
            DialogService.Close();
            StateHasChanged();
            await DialogService.OpenAsync<RefreshLibraryProgressDialog>("Refreshing library...");
            await Grid.Reload();
            CalculateCounts();
        }
    }
}