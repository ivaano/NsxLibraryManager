using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Pages.Components;
using Radzen;
using Radzen.Blazor;
using System.Text.Json;
using System.Linq.Dynamic.Core;
using JetBrains.Annotations;

namespace NsxLibraryManager.Pages;
#nullable disable
public partial class Library : IDisposable
{
    [Inject]
    protected IJSRuntime JsRuntime { get; set; } = default!;
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
    private int _pageSize = 100;
    private string _lastUpdated;
    private int _appCount;
    private int _patchCount;
    private int _dlcCount;
    private string _libraryPath = string.Empty;
    private int _selectedTabIndex;
    private bool _isLoading;
    private int _count;
    [CanBeNull] private DataGridSettings _settings;
    private static readonly string SettingsParamaterName = "TitleLibraryGridSettings";
    [CanBeNull]
    public DataGridSettings Settings 
    { 
        get => _settings;
        set
        {
            if (_settings != value)
            {
                _settings = value;
                InvokeAsync(SaveStateAsync);
            }
        }
    }
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await InitialLoad();
    }
    
    private void LoadSettings(DataGridLoadSettingsEventArgs args)
    {
        if (Settings != null)
        {
            args.Settings = Settings;
        }
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadStateAsync();
            StateHasChanged();
        }
    }
    
    private async Task LoadStateAsync()
    {
        await Task.CompletedTask;

        var result = await JsRuntime.InvokeAsync<string>("window.localStorage.getItem", SettingsParamaterName);
        if (!string.IsNullOrEmpty(result))
        {
            _settings = JsonSerializer.Deserialize<DataGridSettings>(result);
            if (_settings is { PageSize: not null })
            {
                _pageSize = _settings.PageSize.Value;
            }
        }
    }
    
    private async Task SaveStateAsync()
    {
        await Task.CompletedTask;
        await JsRuntime.InvokeVoidAsync("window.localStorage.setItem", SettingsParamaterName, JsonSerializer.Serialize(Settings));
    }

    private Task InitialLoad()
    {
        _isLoading = true;
        //_libraryTitles = await DataService.GetLibraryTitlesAsync();
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
        _isLoading = false;
        return Task.CompletedTask;
    }

    private async Task LoadData(LoadDataArgs args)
    {
        _isLoading = true;
        await Task.Yield();
        
        var query = DataService.GetLibraryTitlesQueryableAsync();

        if (!string.IsNullOrEmpty(args.Filter))
        {
            query = query.Where(args.Filter);
        }
      
        _count = query.Count();
        var skip = args.Skip ?? 0;
        var take = args.Top ?? 100;
        if (!string.IsNullOrEmpty(args.OrderBy))
        {
            _libraryTitles = query.OrderBy(args.OrderBy).Skip(skip).Take(take).ToList();

        }
        else
        {
            _libraryTitles = query.OrderBy(x => x.TitleName).Skip(skip).Take(take).ToList();
        }

        
        _isLoading = false;
    }

    private void UpdateLastUpdate()
    {
        var query = DataService.GetLibraryTitlesQueryableAsync();
         var first = query.OrderByDescending(x => x.LastUpdated).FirstOrDefault()?.LastUpdated.ToString("g");
        _lastUpdated = first ?? "never";
    }
    
    private void CalculateCounts()
    {
        try
        {
            var query = DataService.GetLibraryTitlesQueryableAsync();
            _appCount = query.Count(x => x.Type == TitleLibraryType.Base);
            query = DataService.GetLibraryTitlesQueryableAsync();
            _patchCount = query.Count(x => x.Type == TitleLibraryType.Update);
            query = DataService.GetLibraryTitlesQueryableAsync();
            _dlcCount = query.Count(x => x.Type == TitleLibraryType.DLC);
            /*
            var libTitleList = _libraryTitles.ToList();
        
            _appCount = libTitleList.Count(x => x.Type == TitleLibraryType.Base);
            _patchCount = libTitleList.Count(x => x.Type == TitleLibraryType.Update);
            _dlcCount = libTitleList.Count(x => x.Type == TitleLibraryType.DLC);
            */
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
            await InitialLoad();
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
            await InitialLoad();
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
    
    public async Task OpenDetails(LibraryTitle title)
    {
        await DialogService.OpenAsync<Title>($"{title.TitleName}",
                new Dictionary<string, object>() { { "TitleId", title.TitleId } },
                new DialogOptions() { Width = "80%", Height = "768px", CloseDialogOnEsc = true, CloseDialogOnOverlayClick = true, Draggable = true });
    }

    private void TabOnChange(int index)
    {
         
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);

    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _grid.Dispose();
            _libraryTitles = default!;
        }
    }
}