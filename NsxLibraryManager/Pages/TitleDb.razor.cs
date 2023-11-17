using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NsxLibraryManager.Models;
using NsxLibraryManager.Pages.Components;
using NsxLibraryManager.Services;
using Radzen;
using Radzen.Blazor;
using System.Linq.Dynamic.Core;

namespace NsxLibraryManager.Pages;

public partial class TitleDb
{
    [Inject]
    protected IJSRuntime JsRuntime { get; set; } = default!;
    
    [Inject]
    protected IDataService DataService { get; set; } = default!;
  
    [Inject]
    protected DialogService DialogService { get; set; } = default!;

    private RadzenDataGrid<RegionTitle> grid;
    private IEnumerable<RegionTitle> regionTitles;
    private DataGridSettings? _settings;
    private readonly IEnumerable<int> _pageSizeOptions = new[] { 25, 50, 100 };
    private int _pageSize = 100;
    private static readonly string SettingsParamaterName = "TitleDbGridSettings";
    private int _count;
    private bool _isLoading;
    private string _lastUpdated = "never";
    private IEnumerable<string> _allRegions;
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

        var regionList = new List<string>()
        {
                "US"

        };
        _allRegions = regionList.AsEnumerable();
        
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
        await JsRuntime.InvokeAsync<string>("console.log", "LoadData");
        
        var query = await DataService.GetTitleDbRegionTitlesQueryableAsync("US");
        

        if (!string.IsNullOrEmpty(args.Filter))
        {
            query = query.Where(args.Filter);
        }

        _count = query.Count();

        if (!string.IsNullOrEmpty(args.OrderBy))
        {
            regionTitles =
                    await Task.FromResult(
                            query.OrderBy(args.OrderBy).Skip(args.Skip.Value).Take(args.Top.Value).ToList());
        }
        else
        {
            regionTitles =
                    await Task.FromResult(
                            query.OrderBy(x => x.Name).Skip(args.Skip.Value).Take(args.Top.Value).ToList());
        }

        _isLoading = false;
    } 
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JsRuntime.InvokeAsync<string>("console.log", "OnAfterRenderAsync");
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
            if (_settings.PageSize.HasValue)
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