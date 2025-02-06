using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Dto;
using Radzen;
using Radzen.Blazor;

namespace NsxLibraryManager.Pages;

public partial class MissingDlcUpdates : ComponentBase
{
    [Inject]
    protected ITitleLibraryService TitleLibraryService { get; set; } = null!;
    
    [Inject]
    protected DialogService DialogService { get; set; } = null!;
    
    private IEnumerable<LibraryTitleDto> _libraryTitles = default!;
    
    //grid
    private RadzenDataGrid<LibraryTitleDto> _grid = null!;
    private readonly IEnumerable<int> _pageSizeOptions = [10, 20, 30, 50, 100];
    private int _pageSize = 100;
    private int _count;
    private bool _isLoading = true;
    
    private string _lastUpdated = string.Empty;
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await InitialLoad();
    }

    private async Task InitialLoad()
    {
        var lastUpdated = await TitleLibraryService.GetLastLibraryUpdateAsync();
        _lastUpdated = lastUpdated?.DateUpdated.ToString("MM/dd/yyyy h:mm tt") ?? "Never";
    }
    
    private async Task LoadData(LoadDataArgs args)
    {
        _isLoading = true;
        await Task.Yield();
        var titles = await TitleLibraryService.GetDlcTitlesWithMissingLastUpdate(args);
        _count = titles.Count;
        _libraryTitles = titles.Titles;
        _isLoading = false;
    }
    
    private async Task OpenDetails(LibraryTitleDto title)
    {
        await DialogService.OpenAsync<Title>($"{title.TitleName}",
            new Dictionary<string, object>() { { "TitleId", title.ApplicationId } },
            new DialogOptions() { Width = "90%", Height = "768px", CloseDialogOnEsc = true, CloseDialogOnOverlayClick = true, Draggable = true, Style = "background:var(--rz-base-900)"});
    }
    
}