using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Pages.Components;
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

    private readonly IEnumerable<int> _pageSizeOptions = new[] { 10, 20, 30, 50, 100 };
    private IEnumerable<LibraryTitle> _libraryTitles;
    private IEnumerable<LibraryTitle> _missingDlcs;
    private IEnumerable<LibraryTitle> _missingUpdates;
    private RadzenDataGrid<LibraryTitle> _grid;
    private RadzenDataGrid<LibraryTitle> _updatesGrid;
    private RadzenDataGrid<LibraryTitle> _dlcGrid;
    private string _lastUpdated;
    private int _appCount;
    private int _patchCount;
    private int _dlcCount;
    private string _libraryPath = string.Empty;
    private int _selectedTabIndex;
    
    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        _libraryTitles = await DataService.GetLibraryTitlesAsync();
        _libraryPath = TitleLibraryService.GetLibraryPath();
        CalculateCounts();
        UpdateLastUpdate();
        var titles = DataService.GetLibraryTitlesQueryableAsync();
        _missingDlcs = titles
                .Where(x => x.Type == TitleLibraryType.Base)
                .Where(x => x.AvailableDlcs != x.OwnedDlcs)
                .ToList();
        titles = DataService.GetLibraryTitlesQueryableAsync();
        _missingUpdates = titles
                .Where(x => x.Type == TitleLibraryType.Base)
                .Where(x => x.AvailableVersion != x.LastOwnedVersion)
                .ToList();
    }

    private void UpdateLastUpdate()
    {
        var first = _libraryTitles.FirstOrDefault()?.LastUpdated.ToString("g");
        _lastUpdated = first ?? "never";
    }
    
    private void CalculateCounts()
    {
        try
        {
            var libTitleList = _libraryTitles.ToList();
        
            _appCount = libTitleList.Count(x => x.Type == TitleLibraryType.Base);
            _patchCount = libTitleList.Count(x => x.Type == TitleLibraryType.Update);
            _dlcCount = libTitleList.Count(x => x.Type == TitleLibraryType.DLC);
        } catch (Exception)
        {
            _appCount = 0;
            _patchCount = 0;
            _dlcCount = 0;
        }

    }

    private async Task RefreshLibrary()
    {
        var confirmationResult = await DialogService.Confirm(
            "This will only add/delete new titles since last refresh Are you sure?", "Refresh Library",
            new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" });
        if (confirmationResult is true)
        {
            DialogService.Close();
            StateHasChanged();
            var paramsDialog = new Dictionary<string, object>();
            var dialogOptions = new DialogOptions()
                    { ShowClose = false, CloseDialogOnOverlayClick = false, CloseDialogOnEsc = false };
            await DialogService.OpenAsync<RefreshLibraryProgressDialog>(
                    "Refreshing library...", paramsDialog, dialogOptions);
            await LoadData();
            switch (_selectedTabIndex)
            {
                case 0:
                    await _grid.Reload();
                    break;
                case 1:
                    await _dlcGrid.Reload();
                    break;
                case 2:
                    await _updatesGrid.Reload();
                    break;
            }
        }
    }

    private async Task ReloadLibrary()
    {
        var confirmationResult = await DialogService.Confirm(
                "This will reload all titles to the db Are you sure?", "Reload Library",
                new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" });
        if (confirmationResult is true)
        {
            DialogService.Close();
            StateHasChanged();
            var paramsDialog = new Dictionary<string, object>();
            var dialogOptions = new DialogOptions()
                    { ShowClose = false, CloseDialogOnOverlayClick = false, CloseDialogOnEsc = false };
            await DialogService.OpenAsync<ReloadLibraryProgressDialog>(
                    "Reloading library...", paramsDialog, dialogOptions);
            await DialogService.OpenAsync<RefreshPatchesProgressDialog>(
                    "Processing Updates", paramsDialog, dialogOptions);
            await DialogService.OpenAsync<RefreshDlcProgressDialog>(
                    "Processing Dlcs", paramsDialog, dialogOptions);
            await LoadData();
            switch (_selectedTabIndex)
            {
                case 0:
                    await _grid.Reload();
                    break;
                case 1:
                    await _dlcGrid.Reload();
                    break;
                case 2:
                    await _updatesGrid.Reload();
                    break;
            }
        }
    }

    private void TabOnChange(int index)
    {
         
    }
}