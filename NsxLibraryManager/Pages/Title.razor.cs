using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Models;
using NsxLibraryManager.Services;

namespace NsxLibraryManager.Pages;

public partial class Title
{
    [Inject]
    protected IDataService DataService { get; set; }
    [Inject]
    protected ITitleLibraryService TitleLibraryService { get; set; }
    public string LibraryPath { get; set; } = string.Empty;
    
    public LibraryTitle LibraryTitle { get; set; }
    
    [Parameter]
    public string? TitleId { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        LibraryPath = TitleLibraryService.GetLibraryPath();
        LibraryTitle = TitleLibraryService.GetTitle(TitleId);

    }


}