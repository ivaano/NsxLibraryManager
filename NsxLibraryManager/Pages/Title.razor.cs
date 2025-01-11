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
using NsxLibraryManager.Providers;
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
    private BooleanProvider myBooleanProvider = new BooleanProvider();

    //dlc grid
    private RadzenDataGrid<DlcDto> dlcGrid;
    private bool dlcIsLoading;
    private int dlcCount;
    private IEnumerable<DlcDto> dlcs;
    
    //updates grid
    private RadzenDataGrid<UpdateDto> updatesGrid;
    private bool updatesIsLoading;
    private int updatesCount;
    private IEnumerable<UpdateDto> updates;


    private async Task LiveLoadDlcData(LoadDataArgs args)
    {
        dlcIsLoading = true;
        await Task.Yield();
        if (TitleId is null) return;
        var query = await SqlTitleLibraryService.GetTitleDlcsAsQueryable(TitleId);
        
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

    private async Task LoadDlcData(LoadDataArgs args)
    {
        dlcIsLoading = true;
        await Task.Yield();
        if (LibraryTitle?.Dlc is null) return;
        var libraryApplicationIds = LibraryTitle?.OwnedDlcs?
            .ToLookup(x => x.ApplicationId) ??
                                    new List<DlcDto>().ToLookup(p => p.ToString(), p => p);
        
        var query = LibraryTitle.Dlc.Select(t => new DlcDto
        {
            ApplicationId = t.ApplicationId,
            OtherApplicationId = t.OtherApplicationId,
            TitleName = t.TitleName,
            FileName = libraryApplicationIds.Contains(t.ApplicationId) ? 
                libraryApplicationIds[t.ApplicationId].First().FileName : 
                null,
            Version = libraryApplicationIds.Contains(t.ApplicationId) ? 
                libraryApplicationIds[t.ApplicationId].First().Version : 
                null,
            Size = t.Size,
            Owned = libraryApplicationIds.Contains(t.ApplicationId)
        }).AsQueryable();
        
        if (!string.IsNullOrEmpty(args.Filter))
        {
            query = query.Where(args.Filter);
        }
        
        if (!string.IsNullOrEmpty(args.OrderBy))
        {
            query = query.OrderBy(args.OrderBy);
        }
        dlcCount = query.Count();
        dlcs = query.Skip(args.Skip.Value).Take(args.Top.Value).ToList();
        dlcIsLoading = false;
    }

    private async Task LoadUpdateData(LoadDataArgs args)
    {
        updatesIsLoading = true;
        await Task.Yield();
        if (LibraryTitle?.Updates is null) return;
        var libraryApplicationIds = LibraryTitle?.OwnedUpdates?
                                        .ToLookup(x => x.ApplicationId) ??
                                    new List<UpdateDto>().ToLookup(p => p.ToString(), p => p);
        
        
        var query = LibraryTitle.Updates.Select(t => new UpdateDto
        {
            ApplicationId = t.ApplicationId,
            OtherApplicationId = t.OtherApplicationId,
            TitleName = t.TitleName,
            FileName = libraryApplicationIds.Contains(t.ApplicationId) ? 
                libraryApplicationIds[t.ApplicationId].First().FileName : 
                null,
            Version = libraryApplicationIds.Contains(t.ApplicationId) ? 
                libraryApplicationIds[t.ApplicationId].First().Version : 
                null,
            Size = t.Size,
            Owned = libraryApplicationIds.Contains(t.ApplicationId)
        }).AsQueryable();
        
        if (!string.IsNullOrEmpty(args.Filter))
        {
            query = query.Where(args.Filter);
        }
        
        if (!string.IsNullOrEmpty(args.OrderBy))
        {
            query = query.OrderBy(args.OrderBy);
        }
        
        updatesCount = query.Count();
        updates = query.Skip(args.Skip.Value).Take(args.Top.Value).ToList();
        updatesIsLoading = false;
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
    }


}