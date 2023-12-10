using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Services.Interface;
using Radzen;

namespace NsxLibraryManager.Pages;

public partial class GameGrid
{
    [Inject] 
    protected IDataService DataService { get; set; } = default!;
    [Inject]
    protected DialogService DialogService { get; set; } = default!;
    private IEnumerable<LibraryTitle>? _games;
    private int _count;

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }
    
    private Task LoadData()
    {

        var libraryTitles = DataService.GetLibraryTitlesQueryableAsync();
        var baseGames = libraryTitles.Where(o => o.Type == TitleLibraryType.Base);
        _count = baseGames.Count();
        _games = baseGames
                .OrderBy(t => t.TitleName)
                .ToList();

        return Task.CompletedTask;
    }

    public async Task OpenDetails(LibraryTitle title)
    {
        await DialogService.OpenAsync<Title>($"{title.TitleName}",
                new Dictionary<string, object>() { { "TitleId", title.TitleId } },
                new DialogOptions() { Width = "80%", Height = "768px", CloseDialogOnEsc = true, CloseDialogOnOverlayClick = true, Draggable = true });
    }


}