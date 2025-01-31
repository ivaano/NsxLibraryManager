using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NsxLibraryManager.Models.Dto;
using NsxLibraryManager.Services.Interface;
using Radzen;
using Radzen.Blazor;

namespace NsxLibraryManager.Pages;

public partial class Library : IDisposable
{
    [Inject]
    protected IJSRuntime JsRuntime { get; set; } = null!;
    
    [Inject]
    protected ITitleLibraryService TitleLibraryService { get; set; } = null!;
    
    [Inject]
    protected DialogService DialogService { get; set; } = null!;
    
    //grid
    private DataGridSettings _settings = null!;
    private RadzenDataGrid<LibraryTitleDto> _grid = null!;
    private IEnumerable<LibraryTitleDto> _libraryTitles = null!;
    private bool _isLoading;
    private readonly IEnumerable<int> _pageSizeOptions = [10, 20, 30, 50, 100];
    private int _pageSize = 100;
    private int _count;
    private IEnumerable<string>? _selectedCategories;

    private int _baseCount;
    private int _patchCount;
    private int _dlcCount;
    private string _libraryPath = string.Empty;
    private string _lastUpdated = string.Empty;

    private IEnumerable<string> _categories = [];

    private const string SettingsParamaterName = "SqlLibraryGridSettings";
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
            StateHasChanged();    
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
        await JsRuntime.InvokeVoidAsync("window.localStorage.setItem", SettingsParamaterName, 
            JsonSerializer.Serialize<DataGridSettings>(Settings));
        
        await Task.CompletedTask;
    }
    

    
    private async Task InitialLoad()
    {
        var lastUpdated = await TitleLibraryService.GetLastLibraryUpdateAsync();
        _libraryPath = lastUpdated?.LibraryPath ?? string.Empty;
        _baseCount = lastUpdated?.BaseTitleCount ?? 0;
        _patchCount = lastUpdated?.UpdateTitleCount ?? 0;
        _dlcCount = lastUpdated?.DlcTitleCount ?? 0;
        _lastUpdated = lastUpdated?.DateUpdated.ToString("MM/dd/yyyy h:mm tt") ?? "Never";

        var categoriesResult = await TitleLibraryService.GetCategoriesAsync();
        if (categoriesResult.IsSuccess)
        {
            _categories =categoriesResult.Value;
        } 
    }

    private async Task LoadData(LoadDataArgs args)
    {
        _isLoading = true;
        await Task.Yield();
        
        if (_selectedCategories is not null)
        {
            var filterList = args.Filters?.ToList() ?? [];
            var categoryFilters = _selectedCategories.Select(c => new FilterDescriptor()
            {
                Property = "Category", 
                FilterOperator = FilterOperator.Contains, 
                FilterValue = c
            }).ToList();
            filterList.AddRange(categoryFilters);
            args.Filters = filterList;
        }
        
        var titles = await TitleLibraryService.GetTitles(args);
        if (titles.IsSuccess)
        {
            _libraryTitles = titles.Value.Titles;
            _count = titles.Value.Count;
        }
        _isLoading = false;
    }
    
    private async Task OnSelectedCategoriesChange(object value)
    {
        await _grid.Reload();
    }
    
    private async Task OpenDetails(LibraryTitleDto title)
    {
        await DialogService.OpenAsync<Title>($"{title.TitleName}",
            new Dictionary<string, object>() { { "TitleId", title.ApplicationId } },
            new DialogOptions() { Width = "90%", Height = "768px", CloseDialogOnEsc = true, CloseDialogOnOverlayClick = true, Draggable = true, Style = "background:var(--rz-base-900)"});
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
            _libraryTitles = null!;
            _settings = null!;
            TitleLibraryService = null!;
        }
    }
}