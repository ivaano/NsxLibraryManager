using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NsxLibraryManager.Extensions;
using NsxLibraryManager.Providers;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.Shared.Enums;
using Radzen;
using Radzen.Blazor;

namespace NsxLibraryManager.Pages;

public partial class Title
{
    [Inject]
    protected ITitleLibraryService TitleLibraryService { get; set; } = default!;
    
    [Inject] 
    private IJSRuntime JsRuntime { get; set; } = null!;
    
    [Inject]
    private ISettingsService SettingsService { get; set; } = null!;
    
    [Parameter]
    public string? TitleId { get; set; }

    //carousel
    private RadzenCarousel _carousel = default!;
    private bool _auto = true;
    private double _interval = 4000;
    private bool _started = true;
    private int _selectedIndex;
    
    //readmore/less
    private bool IsExpanded { get; set; }
    private string MaxHeight => IsExpanded ? "none" : "300px";
    private ElementReference textRef;

    private string TextToDisplay => IsExpanded ? HtmlDescription : GetTruncatedText();
    private void Toggle()
    {
        if (_started)
        {
            _carousel.Stop();
        }
        else
        {
            _carousel.Start();
        }

        _started = !_started;
    }
    private LibraryTitleDto? LibraryTitleDto { get; set; }
    private string HtmlDescription { get; set; } = string.Empty;
    private readonly BooleanProvider _myBooleanProvider = new();
    private AgeRatingAgency AgeRatingAgency { get; set; }

    //dlc grid
    private RadzenDataGrid<DlcDto>? _dlcGrid;
    private bool _dlcIsLoading;
    private int _dlcCount;
    private IEnumerable<DlcDto>? _dlcs;
    
    //updates grid
    private RadzenDataGrid<UpdateDto>? _updatesGrid;
    private bool _updatesIsLoading;
    private int _updatesCount;
    private IEnumerable<UpdateDto>? _updates;

    
    private string GetTruncatedText()
    {
        const int maxLength = 900;
        return HtmlDescription.Length > maxLength ? string.Concat(HtmlDescription.AsSpan(0, maxLength), "...") : HtmlDescription;
    }

    private void ToggleText()
    {
        IsExpanded = !IsExpanded;
    }

    private async Task LoadDlcData(LoadDataArgs args)
    {
        _dlcIsLoading = true;
        await Task.Yield();
        if (LibraryTitleDto?.Dlc is null) return;
        var libraryApplicationIds = LibraryTitleDto?.OwnedDlcs?
            .ToLookup(x => x.ApplicationId) ??
                                    new List<DlcDto>().ToLookup(p => p.ToString(), p => p);
        
        var query = LibraryTitleDto?.Dlc.Select(t => new DlcDto
        {
            ApplicationId = t.ApplicationId,
            OtherApplicationId = t.OtherApplicationId,
            TitleName = t.TitleName,
            FileName = libraryApplicationIds.Contains(t.ApplicationId) ? 
                libraryApplicationIds[t.ApplicationId].First().FileName : 
                null,
            Version = libraryApplicationIds.Contains(t.ApplicationId) ? 
                libraryApplicationIds[t.ApplicationId].First().Version : 
                0,
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

        _dlcCount = query.Count();
        _dlcs = query.Skip(args.Skip.Value).Take(args.Top.Value).ToList();

        _dlcIsLoading = false;
    }
    

    private async Task LoadUpdateData(LoadDataArgs args)
    {
        _updatesIsLoading = true;
        await Task.Yield();
        if (LibraryTitleDto?.Updates is null) return;
        var libraryApplicationIds = LibraryTitleDto?.OwnedUpdates?
                                        .ToLookup(x => x.Version) ??
                                    new List<UpdateDto>().ToLookup(p => p.Version, p => p);
        
        var update = LibraryTitleDto?.Updates.FirstOrDefault();
        if (update is null)
        {
            _updatesCount = 0;
            _updatesIsLoading = false;
            return;
        }
        
        var query = LibraryTitleDto?.Versions?.Select(v => new UpdateDto
        {
            ApplicationId = update.ApplicationId,
            OtherApplicationId = update.OtherApplicationId,
            TitleName = update.TitleName,
            Date = v.VersionDate,
            FileName = libraryApplicationIds.Contains(v.VersionNumber) ? 
                libraryApplicationIds[v.VersionNumber].First().FileName : 
                null,
            Version = v.VersionNumber,
            ShortVersion = v.ShortVersionNumber,
            Size = libraryApplicationIds.Contains(v.VersionNumber) ? 
                libraryApplicationIds[v.VersionNumber].First().Size : 
                0,
            Owned = libraryApplicationIds.Contains(v.VersionNumber)
        }).AsQueryable();
        
        if (!string.IsNullOrEmpty(args.Filter))
        {
            query = query.Where(args.Filter);
        }
        
        if (!string.IsNullOrEmpty(args.OrderBy))
        {
            query = query.OrderBy(args.OrderBy);
        }
        
        _updatesCount = query.Count();
        _updates = query.Skip(args.Skip.Value).Take(args.Top.Value).ToList();
        _updatesIsLoading = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
    }
    
    protected override async Task OnParametersSetAsync()
    {
        await LoadTitle();
    }
    
    private async Task LoadTitle()
    {
        
        if (TitleId is null) return;
        var titleResult = await TitleLibraryService.GetTitleByApplicationId(TitleId);
        if (titleResult.IsSuccess)
        {
            var title = titleResult.Value;
            HtmlDescription = new MarkupString(title.Description.Text2Html()).Value;
            LibraryTitleDto = title;
            AgeRatingAgency = SettingsService.GetUserSettings().AgeRatingAgency;
        }
    }
}