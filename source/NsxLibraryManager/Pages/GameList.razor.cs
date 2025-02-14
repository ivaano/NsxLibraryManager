using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.Shared.Enums;
using Radzen;
using Radzen.Blazor;

namespace NsxLibraryManager.Pages;

public partial class GameList
{
    [Inject] 
    protected ITitleLibraryService TitleLibraryService { get; set; } = default!;
    [Inject]
    private ISettingsService SettingsService { get; set; } = null!;

    public bool ShowDlcInfo { get; set; } = false;

    private readonly string _pagingSummaryFormat = "Displaying page {0} of {1} (total {2} games)";
    private readonly int _pageSize = 5;
    private int _count;
    private bool _isLoading;
    private IEnumerable<LibraryTitleDto> _games = default!;
    private FilterDescriptor? _filterDescriptor;
    private const string ApplicationIdPattern = "^[0-9A-F]{16}$";
    private static readonly RegexOptions RegexFlags = RegexOptions.IgnoreCase | RegexOptions.Compiled;
    private RadzenPager _pager = default!;
    private AgeRatingAgency AgeRatingAgency { get; set; }
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await InitialLoad();
    }

    private async Task InitialLoad()
    {
        var loadDataArgs = new LoadDataArgs
        {
            Top = _pageSize,
            Skip = 0
        };
        AgeRatingAgency = SettingsService.GetUserSettings().AgeRatingAgency;
        await LoadData(loadDataArgs);
    }

    private async Task PageChanged(PagerEventArgs args)
    {
        var loadDataArgs = new LoadDataArgs
        {
            Top = args.Top,
            Skip = args.Skip,
            OrderBy = "TitleName",
            Filters = _filterDescriptor is not null ? [_filterDescriptor] : null
        };
        await LoadData(loadDataArgs);
    }

    private async Task LoadData(LoadDataArgs args)
    {
        _isLoading = true;
        const string baseFilter = "ContentType = 128";
        args.Filter = string.IsNullOrEmpty(args.Filter) ? 
            baseFilter : $"({baseFilter}) and ({args.Filter})";
        
        
        var loadData = await TitleLibraryService.GetTitles(args);
        if (loadData.IsSuccess)
        {
            _games = loadData.Value.Titles;
            _count = loadData.Value.Count;            
        }
        _isLoading = false;
    }

    private async Task OnFilterChange(string? value, string name)
    {
        if (string.IsNullOrEmpty(value))
        {
            await InitialLoad();
            _filterDescriptor = null;
            await _pager.FirstPage();
            return;
        }
        
        _filterDescriptor = new FilterDescriptor
        {
            Property = Regex.IsMatch(value, ApplicationIdPattern, RegexFlags) 
                ? "ApplicationId" 
                : "TitleName",
            FilterOperator = Regex.IsMatch(value, ApplicationIdPattern, RegexFlags)
                ? FilterOperator.Equals 
                : FilterOperator.Contains,
            FilterValue = value
        };
        
        var loadDataArgs = new LoadDataArgs
        {
            Top = _pageSize,
            Skip = 0,
            Filters = [_filterDescriptor],
            OrderBy = "TitleName"
        };

        await LoadData(loadDataArgs);
    }


    public void ShowDlcInfoToggle(string titleId)
    {
        ShowDlcInfo = !ShowDlcInfo;
    }
}