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
    private DataGridSettings _settings;
    private IEnumerable<int> pageSizeOptions = new[] { 25, 50, 100 };
    private int pageSize = 25;
    private static readonly string _settingsParamaterName = "TitleDbGridSettings";
    private int count = 0;
    private bool isLoading = false;
    private bool loaded = false;
    private const string titleDbSettings = "TitledbGridSettings";

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


    private void LoadSettings(DataGridLoadSettingsEventArgs args)
    {
        if (Settings != null)
        {
            args.Settings = Settings;
        }
    }
    
    private async Task LoadData(LoadDataArgs args)
    {
        isLoading = true;

        //await Task.Yield();
        //var gridSettings = await DataService.LoadDataGridStateAsync(titleDbSettings);

        
        var query = await DataService.GetTitleDbRegionTitlesQueryableAsync("US");
        

        if (!string.IsNullOrEmpty(args.Filter))
        {
            query = query.Where(args.Filter);
        }
/*
        if (!string.IsNullOrEmpty(args.OrderBy))
        {
            query = query.OrderBy(args.OrderBy);
        }
*/
        //query.Where(x => x.Type == TitleLibraryType.DLC);
        // Simulate async data loading
        //await Task.Delay(2000);
        count = query.Count();
/*
        regionTitles = await Task.FromResult(query
                .Where(x => x.Type == TitleLibraryType.Unknown)
                .Skip(args.Skip.Value).Take(args.Top.Value).ToList());
        */
        regionTitles =  await Task.FromResult(query.OrderBy(x => x.Name).Skip(args.Skip.Value).Take(args.Top.Value).ToList());
        
        isLoading = false;

        loaded = true;

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

        var result = await JsRuntime.InvokeAsync<string>("window.localStorage.getItem", _settingsParamaterName);
        if (!string.IsNullOrEmpty(result))
        {
            _settings = JsonSerializer.Deserialize<DataGridSettings>(result);
            if (_settings.PageSize.HasValue)
            {
                pageSize = _settings.PageSize.Value;
            }
        }
    }
    
    private async Task SaveStateAsync()
    {
        await Task.CompletedTask;
        //await DataService.SaveDataGridStateAsync(titleDbSettings, Settings);
        await JsRuntime.InvokeVoidAsync("window.localStorage.setItem", _settingsParamaterName, JsonSerializer.Serialize(Settings));
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