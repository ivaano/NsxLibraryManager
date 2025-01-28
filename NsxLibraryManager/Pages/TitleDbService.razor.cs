using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NsxLibraryManager.Models.Dto;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.ViewModels.TitleDb;
using Radzen;
using Radzen.Blazor;

namespace NsxLibraryManager.Pages;

public partial class TitleDbService : ComponentBase
{
    [Inject]
    protected IJSRuntime JsRuntime { get; set; } = default!;
    [Inject]
    protected DialogService DialogService { get; set; } = default!;
    
    [Inject]
    protected ITitledbService TitledbService { get; set; } = null!;
    
    private RadzenDataGrid<LibraryTitleDto> _grid;
    private IEnumerable<LibraryTitleDto> _titles =  null!;
    private readonly IEnumerable<int> _pageSizeOptions = [25, 50, 100];
    private int _pageSize = 100;
    private int _count = 0;
    private bool _isLoading;
    private string _lastUpdated = "never";
    private string _dbVersion = string.Empty;
    
    private IEnumerable<string> _selectedCategories;
    private IEnumerable<string> _categories;
    private List<FilterDescriptor> categoryFilters = [];
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        var categoriesResult = await TitledbService.GetCategoriesAsync();

        if (categoriesResult.IsSuccess)
        {
            _categories =categoriesResult.Value;
        }        
    }
    
    private async Task LoadData(LoadDataArgs args)
    {
        _isLoading = true;
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
}
