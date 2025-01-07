using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Models.Dto;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Utils;

namespace NsxLibraryManager.Pages;

public partial class Title
{
    //[Inject]
    //protected ITitleLibraryService TitleLibraryService { get; set; } = default!;
    //[Inject]
    //protected ITitleDbService TitleDbService { get; set; } = default!;
    [Parameter]
    public string? TitleId { get; set; }

    private IEnumerable<GameVersions> GameVersions { get; set; } = new List<GameVersions>();
    private IEnumerable<Dlc> GameDlcs { get; set; } = new List<Dlc>();

    private LibraryTitle? LibraryTitle { get; set; }
    private string GameFileSize { get; set; } = string.Empty;
    private string HtmlDescription { get; set; } = string.Empty;

    
    protected override async Task OnParametersSetAsync()
    {
        await LoadTitle();
    }
    
    private async Task LoadTitle()
    {
        /*
        if (TitleId is null) return;  
        //LibraryTitle = TitleLibraryService.GetTitle(TitleId);
        if (LibraryTitle is not null)
        {
            var sizeInBytes = LibraryTitle.Size ?? 0;
            GameFileSize = sizeInBytes.ToHumanReadableBytes();
            HtmlDescription = new MarkupString(LibraryTitle.Description.Text2Html()).Value;
            var titlePatches = TitleDbService.GetVersions(LibraryTitle.TitleId);
            var titlePatchesList = new List<GameVersions>();
            
            foreach (var patch in titlePatches)
            {
                if (LibraryTitle.OwnedUpdates == null) continue;
                var query = from o in LibraryTitle.OwnedUpdates
                        where o == patch.VersionShifted
                        select o;
                patch.Owned = query.Any();

                titlePatchesList.Add(patch);
            }
            GameVersions = titlePatchesList;
            
            var titleDlcs = await TitleDbService.GetTitleDlc(LibraryTitle.TitleId);
            var titleDlcList = new List<Dlc>();
            foreach (var dlc in titleDlcs)
            {
                if (LibraryTitle.OwnedDlcs == null)
                {
                    dlc.Owned = false;
                    titleDlcList.Add(dlc);
                    continue;
                }
                var query = from o in LibraryTitle.OwnedDlcs
                        where o == dlc.TitleId
                        select o;
                dlc.Owned =  query.Any();

                titleDlcList.Add(dlc);
            }
            GameDlcs = titleDlcList;
        } 
        else
        {
            LibraryTitle = await TitleLibraryService.GetTitleFromTitleDb(TitleId) ?? new LibraryTitle
            {
                    TitleId = TitleId,
                    Size = 0,
                    FileName = string.Empty
            };
        }
        */
    }


}