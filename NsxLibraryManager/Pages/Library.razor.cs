using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Enums;
using NsxLibraryManager.Models;
using NsxLibraryManager.Pages.Components;
using NsxLibraryManager.Services;
using NsxLibraryManager.Services.Interface;
using Radzen;
using Radzen.Blazor;

namespace NsxLibraryManager.Pages;
#nullable disable
public partial class Library
{
    [Inject]
    protected IDataService DataService { get; set; }
    
    [Inject]
    protected ITitleLibraryService TitleLibraryService { get; set; }
   
    [Inject]
    protected DialogService DialogService { get; set; }
    IEnumerable<int> pageSizeOptions = new int[] { 10, 20, 30, 50, 100 };
    
    public IEnumerable<LibraryTitle> LibraryTitles;
    public IEnumerable<LibraryTitle> MissingDlcs;
    public IEnumerable<LibraryTitle> MissingUpdates;
    public RadzenDataGrid<LibraryTitle> Grid;
    public RadzenDataGrid<LibraryTitle> UpdatesGrid;
    public RadzenDataGrid<LibraryTitle> DlcGrid;
    private string lastUpdated;
    public int AppCount = 0;
    public int PatchCount = 0;
    public int DlcCount = 0;
    public string LibraryPath = string.Empty;
    public Dictionary<string, string> GridColumns = new()
    {
        { "TitleId", "Title ID" },
        { "Name", "Name" },
        { "Type", "Type" },
        { "Version", "Version" },
        { "Date", "Date" }
    };
    private int selectedTabIndex = 0;
    
    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        LibraryTitles = await DataService.GetLibraryTitlesAsync();
        LibraryPath = TitleLibraryService.GetLibraryPath();
        CalculateCounts();
        UpdateLastUpdate();
        var titles = DataService.GetLibraryTitlesQueryableAsync();
        MissingDlcs = titles
                .Where(x => x.Type == TitleLibraryType.Base)
                .Where(x => x.AvailableDlcs != x.OwnedDlcs)
                .ToList();
        MissingUpdates = titles
                .Where(x => x.Type == TitleLibraryType.Base)
                .Where(x => x.AvailableVersion != x.LastOwnedVersion)
                .ToList();
    }

    private void UpdateLastUpdate()
    {
        var first = LibraryTitles.FirstOrDefault()?.LastUpdated.ToString("g");
        lastUpdated = first ?? "never";
    }
    
    private void CalculateCounts()
    {
        try
        {
            var libTitleList = LibraryTitles.ToList();
        
            AppCount = libTitleList.Count(x => x.Type == TitleLibraryType.Base);
            PatchCount = libTitleList.Count(x => x.Type == TitleLibraryType.Update);
            DlcCount = libTitleList.Count(x => x.Type == TitleLibraryType.DLC);
        } catch (Exception e)
        {
            AppCount = 0;
            PatchCount = 0;
            DlcCount = 0;
        }

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
            var paramsDialog = new Dictionary<string, object>() { };
            var dialogOptions = new DialogOptions()
                    { ShowClose = false, CloseDialogOnOverlayClick = false, CloseDialogOnEsc = false };
            await DialogService.OpenAsync<RefreshLibraryProgressDialog>(
                    "Refreshing library...", paramsDialog, dialogOptions);
            await DialogService.OpenAsync<RefreshPatchesProgressDialog>(
                    "Processing Updates", paramsDialog, dialogOptions);
            await DialogService.OpenAsync<RefreshDlcProgressDialog>(
                    "Processing Dlcs", paramsDialog, dialogOptions);
            await LoadData();
            switch (selectedTabIndex)
            {
                case 0:
                    await Grid.Reload();
                    break;
                case 1:
                    await DlcGrid.Reload();
                    break;
                case 2:
                    await UpdatesGrid.Reload();
                    break;
            }
        }
    }

    private void TabOnChange(int index)
    {
         
    }
}