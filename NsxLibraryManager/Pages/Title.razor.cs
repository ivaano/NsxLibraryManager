using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Models;
using NsxLibraryManager.Services;
using NsxLibraryManager.Utils;

namespace NsxLibraryManager.Pages;

public partial class Title
{
    [Inject]
    protected IDataService DataService { get; set; }
    [Inject]
    protected ITitleLibraryService TitleLibraryService { get; set; }
    [Inject]
    protected ITitleDbService TitleDbService { get; set; }
    [Parameter]
    public string? TitleId { get; set; }
    
    public LibraryTitle? LibraryTitle { get; set; }
    public string GameFileSize { get; set; } = string.Empty;
    
    protected override async Task OnInitializedAsync()
    {
        if (TitleId is null) return;  
        LibraryTitle = TitleLibraryService.GetTitle(TitleId);

        if (LibraryTitle is null)
        {
            LibraryTitle = await TitleLibraryService.GetTitleFromTitleDb(TitleId);
            if (LibraryTitle is null)
            {
                LibraryTitle = new LibraryTitle
                {
                        TitleId = string.Empty,
                        FileName = string.Empty
                };
            };
        }


        var sizeInBytes = LibraryTitle.Size ?? 0;
        GameFileSize = sizeInBytes.ToHumanReadableBytes();
    }


}