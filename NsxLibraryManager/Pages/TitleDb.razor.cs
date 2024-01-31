using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NsxLibraryManager.Pages.Components;
using Radzen;
using Radzen.Blazor;
using System.Linq.Dynamic.Core;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Services.Interface;

namespace NsxLibraryManager.Pages;

public partial class TitleDb : IDisposable
{
    [Inject]
    protected IJSRuntime JsRuntime { get; set; } = default!;
    
    [Inject]
    protected IDataService DataService { get; set; } = default!;
  
    [Inject]
    protected DialogService DialogService { get; set; } = default!;

    private static readonly string SettingsParamaterName = "TitleDbGridSettings";

    private RadzenDataGrid<RegionTitle> _grid = default!;
    private IEnumerable<RegionTitle> _regionTitles = default!;
    private DataGridSettings? _settings;
    private readonly IEnumerable<int> _pageSizeOptions = new[] { 25, 50, 100 };
    private int _pageSize = 100;
    private int _count;
    private bool _isLoading;
    private string _lastUpdated = "never";

    public DataGridSettings? Settings 
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

        /*
        var regionList = new List<string>()
        {
                "US"

        };
        _allRegions = regionList.AsEnumerable();
        */
    }

    private void LoadSettings(DataGridLoadSettingsEventArgs args)
    {
        if (Settings != null)
        {
            args.Settings = Settings;
        }
    }
    
    private async Task LoadData(LoadDataArgs args)
    {
        _isLoading = true;
        _lastUpdated =  DataService.GetRegionLastUpdate("US").ToString() ?? "never";
        await Task.Yield();
        
        var query = await DataService.GetTitleDbRegionTitlesQueryableAsync("US");
        

        if (!string.IsNullOrEmpty(args.Filter))
        {
            query = query.Where(args.Filter);
        }

        _count = query.Count();
        
        var skip = args.Skip ?? 0;
        var take = args.Top ?? 100;
        if (!string.IsNullOrEmpty(args.OrderBy))
        {

            _regionTitles =
                    await Task.FromResult(
                            query.OrderBy(args.OrderBy).Skip(skip).Take(take).ToList());
        }
        else
        {
            _regionTitles =
                    await Task.FromResult(
                            query.OrderBy(x => x.Name).Skip(skip).Take(take).ToList());
        }

        _isLoading = false;
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

    private async Task OpenDetails(RegionTitle title)
    {
        if (title.TitleId is null) return;
        await DialogService.OpenAsync<TitleDbTitle>($"{title.Name}",
            new Dictionary<string, object>() { { "TitleId", title.TitleId } },
            new DialogOptions() { Width = "80%", Height = "768px", CloseDialogOnEsc = true, CloseDialogOnOverlayClick = true, Draggable = true });
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
            await _grid.Reload();
            StateHasChanged();
        }
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
            _regionTitles = default!;
        }
    }
}