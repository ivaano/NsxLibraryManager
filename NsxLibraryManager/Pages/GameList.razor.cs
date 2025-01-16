using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Models.Dto;
using NsxLibraryManager.Pages.Components;
using NsxLibraryManager.Services.Interface;
using Radzen;

namespace NsxLibraryManager.Pages;

public partial class GameList
{
    [Inject] 
    protected ITitleLibraryService TitleLibraryService { get; set; } = default!;


    public bool ShowDlcInfo { get; set; } = false;

    private readonly string _pagingSummaryFormat = "Displaying page {0} of {1} (total {2} records)";
    private readonly int _pageSize = 10;
    private int _count;
    public bool IsLoading;


    private IEnumerable<LibraryTitleDto> _games = default!;


    private async Task PageChanged(PagerEventArgs args)
    {
        var loadDataArgs = new LoadDataArgs
        {
            Top = args.Top,
            Skip = args.Skip
        };
        await LoadData(loadDataArgs);
    }

    private async Task LoadData(LoadDataArgs args)
    {
        IsLoading = true;

        _games = await TitleLibraryService.GetTitlesAsQueryable(args);
       
        _count = _games.Count();
/*
        var libraryTitles = DataService.GetLibraryTitlesQueryableAsync();
        var baseGames = libraryTitles.Where(o => o.Type == TitleLibraryType.Base);
        _count = baseGames.Count();
        _games = baseGames
            .Take(args.Top ?? _pageSize)
            .OrderBy(t => t.TitleName)
            .Skip(args.Skip ?? 0)
            .ToList();
*/
        IsLoading = false;
    }

    public void ShowDlcInfoToggle(string titleId)
    {
        ShowDlcInfo = !ShowDlcInfo;
    }
}