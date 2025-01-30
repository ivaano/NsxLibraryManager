using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NsxLibraryManager.Models.Dto;
using NsxLibraryManager.Services.Interface;
using Radzen;
using Radzen.Blazor;

namespace NsxLibraryManager.Pages;

public partial class LibraryService : ComponentBase
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
    
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await InitialLoad();
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
}