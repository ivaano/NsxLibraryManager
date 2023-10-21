using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using NsxLibraryManager.Enums;
using NsxLibraryManager.Models;
using NsxLibraryManager.Pages.Components;
using NsxLibraryManager.Services;
using Radzen;

namespace NsxLibraryManager.Pages;

public partial class GameLibrary
{
    
    [Inject]
    protected IDataService DataService { get; set; }

    
    
    public bool ShowDlcInfo { get; set; } = false;

    string pagingSummaryFormat = "Displaying page {0} of {1} (total {2} records)";
    int pageSize = 10;
    int count;
    public bool isLoading;
    private ShowDlc _showDlc;

    
    IEnumerable<LibraryTitle> games;
    
    
    
    async Task PageChanged(PagerEventArgs args)
    {
        var loadDataArgs = new LoadDataArgs { 
                Top = args.Top,
                Skip = args.Skip
        };
        await LoadData(loadDataArgs);
    }

    private async Task LoadData(LoadDataArgs args)
    {
        isLoading = true;

        var libraryTitles = await DataService.GetLibraryTitlesQueryableAsync();
        var baseGames = libraryTitles.Where(o => o.Type == TitleLibraryType.Base);
        count = baseGames.Count();
        games = baseGames
                .Take(args.Top ?? pageSize)
                .OrderBy(t => t.TitleName)
                .Skip(args.Skip ?? 0)
                .ToList();

        isLoading = false;
    }

    public void ShowDlcInfoToggle(string titleId)
    {
        ShowDlcInfo = !ShowDlcInfo;
    }
}