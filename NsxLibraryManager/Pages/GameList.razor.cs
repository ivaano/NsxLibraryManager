using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Models.Dto;
using NsxLibraryManager.Services.Interface;
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
    private readonly int _pageSize = 10;
    private int _count;
    public bool IsLoading;
    private IEnumerable<LibraryTitleDto> _games = default!;
    private FilterDescriptor? _filterDescriptor;
    private const string ApplicationIdPattern = "^[0-9A-F]{16}$";
    private static readonly RegexOptions RegexFlags = RegexOptions.IgnoreCase | RegexOptions.Compiled;
    private RadzenPager pager;
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
        IsLoading = true;
        var loadData = await TitleLibraryService.GetBaseTitles(args);
        _games = loadData.Titles;
        _count = loadData.Count;
        IsLoading = false;
    }

    private async Task OnFilterChange(string value, string name)
    {
        if (string.IsNullOrEmpty(value))
        {
            await InitialLoad();
            _filterDescriptor = null;
            await pager.FirstPage();
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