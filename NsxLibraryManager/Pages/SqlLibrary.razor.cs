using System.Text.Json;
using Microsoft.JSInterop;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Data;
using Radzen;
using Radzen.Blazor;
using System.Linq.Dynamic.Core;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Pages.Components;
using TitleModel = NsxLibraryManager.Models.NsxLibrary.Title;

namespace NsxLibraryManager.Pages;

public partial class SqlLibrary : IDisposable
{
    [Inject]
    protected IJSRuntime JsRuntime { get; set; } = default!;
    
    [Inject]
    protected ITitleLibraryService TitleLibraryService { get; set; }
    
    [Inject]
    protected NsxLibraryDbContext DbContext { get; set; } = default!;
    
    [Inject]
    protected DialogService DialogService { get; set; }
    
    private static readonly string SettingsParamaterName = "SqlLibraryGridSettings";

    [CanBeNull] private DataGridSettings _settings;
    private IEnumerable<TitleModel> _libraryTitles;
    private RadzenDataGrid<TitleModel> _grid;

    private int _selectedTabIndex;
    private string _libraryPath = string.Empty;
    private bool _isLoading;
    private int _baseCount;
    private int _patchCount;
    private int _dlcCount;
    private int _count;
    private int _pageSize = 100;
    private string _lastUpdated;
    
    private readonly IEnumerable<int> _pageSizeOptions = [10, 20, 30, 50, 100];
    private DataGridSettings? Settings 
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
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadStateAsync();
            await Task.Delay(1);
            await _grid.Reload();
            //StateHasChanged();    
        }
    }
    
    private Task InitialLoad()
    {
        _libraryPath = TitleLibraryService.GetLibraryPath();
        CalculateCounts();
        return Task.CompletedTask;
    }

    private void CalculateCounts()
    {
        try
        {
            _baseCount = DbContext.Titles
                .Count(x => x.ContentType == TitleContentType.Base);
            _patchCount = DbContext.Titles
                .Count(x => x.ContentType == TitleContentType.Update);
            _dlcCount = DbContext.Titles
                .Count(x => x.ContentType == TitleContentType.DLC);  
        }
        catch (Exception)
        {
            _baseCount = 0;
            _patchCount = 0;
            _dlcCount = 0;
        }
    }

    private async Task LoadData(LoadDataArgs args)
    {
        _isLoading = true;
        await Task.Yield();

        var query = DbContext.Titles.AsQueryable();
        
        if (!string.IsNullOrEmpty(args.Filter))
        {
            query = query.Where(args.Filter);
        }

        if (!string.IsNullOrEmpty(args.OrderBy))
        {
            query = query.OrderBy(args.OrderBy);
        }

        _count = query.Count();

        _libraryTitles = await Task.FromResult(
            query.
                Skip(args.Skip.Value).
                Take(args.Top.Value).
                ToList());

        _isLoading = false;
    }
    
    
    private async Task LoadStateAsync()
    {
        var result = await JsRuntime.InvokeAsync<string>("window.localStorage.getItem", SettingsParamaterName);
        if (!string.IsNullOrEmpty(result))
        {
            _settings = JsonSerializer.Deserialize<DataGridSettings>(result);
            if (_settings is { PageSize: not null })
            {
                _pageSize = _settings.PageSize.Value;
            }
        }
        await Task.CompletedTask;
    }
    
    private async Task SaveStateAsync()
    {
        await JsRuntime.InvokeVoidAsync("window.localStorage.setItem", SettingsParamaterName, 
            JsonSerializer.Serialize<DataGridSettings>(Settings));
        
        await Task.CompletedTask;
    }
    

    
    private void LoadSettings(DataGridLoadSettingsEventArgs args)
    {
        if (Settings != null)
        {
            args.Settings = Settings;
        }
    }

    private void TabOnChange(int index)
    {
         
    }

    private async Task RefreshLibrary()
    {
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
            await DialogService.OpenAsync<SqlReloadLibraryProgressDialog>(
                "Reloading library...", paramsDialog, dialogOptions);
            /*
            await DialogService.OpenAsync<RefreshPatchesProgressDialog>(
                "Processing Updates", paramsDialog, dialogOptions);
            await DialogService.OpenAsync<RefreshDlcProgressDialog>(
                "Processing Dlcs", paramsDialog, dialogOptions);
                */
            await InitialLoad();
            switch (_selectedTabIndex)
            {
                case 0:
                    await _grid.Reload();
                    break;
                case 1:
                    //await _dlcGrid.Reload();
                    break;
                case 2:
                    //await _updatesGrid.Reload();
                    break;
            }
        }
    }
    
    public async Task OpenDetails(TitleModel title)
    {
        await DialogService.OpenAsync<Title>($"{title.TitleName}",
            new Dictionary<string, object>() { { "TitleId", title.ApplicationId } },
            new DialogOptions() { Width = "80%", Height = "768px", CloseDialogOnEsc = true, CloseDialogOnOverlayClick = true, Draggable = true });
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
        }
    }
}