using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Dto;
using Radzen;
using Radzen.Blazor;

namespace NsxLibraryManager.Pages;

public partial class TitleDb : ComponentBase
{
    private const string SettingsParamaterName = "TitleDbGridSettings";

    [Inject]
    protected IJSRuntime JsRuntime { get; set; } = default!;
    
    [Inject]
    protected DialogService DialogService { get; set; } = default!;
    
    [Inject]
    protected ITitledbService TitledbService { get; set; } = null!;

    private RadzenDataGrid<LibraryTitleDto> _grid = null!;
    private DataGridSettings? _settings;
    private IEnumerable<LibraryTitleDto> _titles =  null!;
    private readonly IEnumerable<int> _pageSizeOptions = [25, 50, 100];
    private int _pageSize = 100;
    private int _count;
    private bool _isLoading;

    private string _lastUpdated = "never";
    private string _dbVersion = string.Empty;
    private IEnumerable<string>? _selectedCategories;
    private IEnumerable<string>? _categories;
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        var categoriesResult = await TitledbService.GetCategoriesAsync();

        if (categoriesResult.IsSuccess)
        {
            _categories =categoriesResult.Value;
        }        
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadStateAsync();
            await Task.Delay(1);
            StateHasChanged();    
        }
    }
    
    private async Task LoadData(LoadDataArgs args)
    {
        _isLoading = true;
        await Task.Yield();
        var dbVersion = TitledbService.GetLatestTitledbVersionAsync();
        if (dbVersion is { IsSuccess: true, Value: var dbHistoryDto })
        {
            _dbVersion = dbHistoryDto.Version;
            _lastUpdated = dbHistoryDto.Date;
        }
        
        var titlesResult = await TitledbService.GetTitles(args, _selectedCategories);
        if (titlesResult.IsSuccess)
        {
            _count = titlesResult.Value.TotalRecords;
            _titles = titlesResult.Value.Titles.ToList();
        }
        else
        {
            _titles = Array.Empty<LibraryTitleDto>();
        }
        _isLoading = false;
    }
    
    private async Task OpenDetails(LibraryTitleDto title)
    {
        await DialogService.OpenAsync<TitleDbTitle>($"{title.TitleName}",
            new Dictionary<string, object>() { { "TitleId", title.ApplicationId } },
            new DialogOptions() { Width = "90%", Height = "768px", CloseDialogOnEsc = true, CloseDialogOnOverlayClick = true, Draggable = true, Style = "background:var(--rz-base-900)"});
    }
    
    private async Task OnSelectedCategoriesChange(object value)
    {
        await _grid.Reload();
    }
    
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
        if (Settings is not null)
            await JsRuntime.InvokeVoidAsync(
                "window.localStorage.setItem",
                SettingsParamaterName,
                JsonSerializer.Serialize(Settings));

        await Task.CompletedTask;
    }
}
