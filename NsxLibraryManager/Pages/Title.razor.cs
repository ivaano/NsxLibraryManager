using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Models.Dto;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Data;
using NsxLibraryManager.Extensions;
using NsxLibraryManager.Models.Dto;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Utils;
using Radzen;
using Radzen.Blazor;

namespace NsxLibraryManager.Pages;

public partial class Title
{
    [Inject]
    protected ISqlTitleLibraryService SqlTitleLibraryService { get; set; }
    [Inject] 
    private IJSRuntime JsRuntime { get; set; } = default!;
    
    [Parameter]
    public string? TitleId { get; set; }

    private IEnumerable<GameVersions> GameVersions { get; set; } = new List<GameVersions>();
    private IEnumerable<Dlc> GameDlcs { get; set; } = new List<Dlc>();

    private LibraryTitleDto? LibraryTitle { get; set; }
    private string HtmlDescription { get; set; } = string.Empty;

    //dlc grid
    private RadzenDataGrid<DlcDto> dlcGrid;
    private bool dlcIsLoading;
    private int dlcCount;
    private IEnumerable<DlcDto> dlcs;


    private async Task LoadDlcData(LoadDataArgs args)
    {
        dlcIsLoading = true;
        await Task.Yield();
        if (TitleId is null) return;
        var query = await SqlTitleLibraryService.GetTitleDlcsAsync(TitleId);
        
        if (!string.IsNullOrEmpty(args.Filter))
        {
            // Filter via the Where method
            query = query.Where(args.Filter);
        }
        
        if (!string.IsNullOrEmpty(args.OrderBy))
        {
            // Sort via the OrderBy method
            query = query.OrderBy(args.OrderBy);
        }
        
        dlcs = query.Skip(args.Skip.Value).Take(args.Top.Value).ToList();

       
        dlcCount = query.Count();
        dlcIsLoading = false;
        //dlcs = await query.ToListAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JsRuntime.InvokeVoidAsync("scrollDialogToTop");
        }
        await base.OnAfterRenderAsync(firstRender);
    }
    
    protected override async Task OnParametersSetAsync()
    {
        await LoadTitle();
    }
    
    private async Task LoadTitle()
    {
        
        if (TitleId is null) return;
        var title = await SqlTitleLibraryService.GetTitleByApplicationId(TitleId);
        HtmlDescription = new MarkupString(title.Description.Text2Html()).Value;

        if (title is null) return;
        LibraryTitle = title;
        /*
        var sizeInBytes = title.Size ?? 0;
        GameFileSize = sizeInBytes.ToHumanReadableBytes();
        HtmlDescription = new MarkupString(title.Description.Text2Html()).Value;
        LibraryTitle = new LibraryTitle
        {
            TitleId = title.ApplicationId,
            TitleName = title.TitleName,
            FileName = title.FileName,
        };
        
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