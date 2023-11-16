using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Models;
using NsxLibraryManager.Models.Dto;
using NsxLibraryManager.Services;
using NsxLibraryManager.Services.Interface;
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
    public IEnumerable<GameVersions> GameVersions { get; set; } = new List<GameVersions>();
    public IEnumerable<Dlc> GameDlcs { get; set; } = new List<Dlc>();

    public LibraryTitle? LibraryTitle { get; set; }
    public string GameFileSize { get; set; } = string.Empty;
    public string HtmlDescription { get; set; } = string.Empty;

    
    protected override async Task OnParametersSetAsync()
    {
        await LoadTitle();
    }
    
    private async Task LoadTitle()
    {
        if (TitleId is null) return;  
        LibraryTitle = TitleLibraryService.GetTitle(TitleId);
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
                try
                {
                    var esta = LibraryTitle.OwnedUpdates.First(s => s.Equals(patch.VersionShifted));
                    patch.Owned = true;
                } 
                catch (Exception)
                {
                    patch.Owned = false;
                }
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
                
                try
                {
                    var esta = LibraryTitle.OwnedDlcs.First(s => s.Equals(dlc.TitleId, StringComparison.InvariantCultureIgnoreCase));
                    dlc.Owned = true;
                } 
                catch (Exception)
                {
                    dlc.Owned = false;
                }
                titleDlcList.Add(dlc);
            }
            GameDlcs = titleDlcList;
        } 
        else
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
    }


}