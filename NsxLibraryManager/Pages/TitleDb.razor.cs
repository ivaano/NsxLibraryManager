using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NsxLibraryManager.Models;
using NsxLibraryManager.Pages.Components;
using NsxLibraryManager.Services;
using Radzen;

namespace NsxLibraryManager.Pages;

public partial class TitleDb
{
    [Inject]
    protected IJSRuntime JsRuntime { get; set; } = default!;
    
    [Inject]
    protected IDataService DataService { get; set; } = default!;
  
    [Inject]
    protected DialogService DialogService { get; set; } = default!;
    
    private IEnumerable<RegionTitle> regionTitles;
    private DataGridSettings _settings;
    public DataGridSettings Settings 
    { 
        get
        {
            return _settings;
        }
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
        regionTitles = await DataService.GetTitleDbRegionTitlesAsync("US");
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

        var result = await JsRuntime.InvokeAsync<string>("window.localStorage.getItem", "Settings");
        if (!string.IsNullOrEmpty(result))
        {
            _settings = JsonSerializer.Deserialize<DataGridSettings>(result);
        }
    }
    
    private async Task SaveStateAsync()
    {
        await Task.CompletedTask;

        await JsRuntime.InvokeVoidAsync("window.localStorage.setItem", "Settings", JsonSerializer.Serialize(Settings));
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