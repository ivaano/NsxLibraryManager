using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Extensions;
using NsxLibraryManager.Providers;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.Shared.Enums;
using Radzen;
using Radzen.Blazor;

namespace NsxLibraryManager.Pages;

public partial class TitleDbTitle
{
    [Inject]
    private ISettingsService SettingsService { get; set; } = null!;
    
    [Inject]
    protected ITitledbService TitledbService { get; set; } = default!;
    
    [Parameter] 
    public string? TitleId { get; set; }
    private LibraryTitleDto? LibraryTitleDto { get; set; }
    
    public string HtmlDescription { get; set; } = string.Empty;
    
    private AgeRatingAgency AgeRatingAgency { get; set; }
    private readonly BooleanProvider _myBooleanProvider = new();

    //carousel
    private RadzenCarousel _carousel = default!;
    private bool auto = true;
    private double interval = 4000;
    private int selectedIndex;
    
    //dlc grid
    private RadzenDataGrid<DlcDto> _dlcGrid;
    private bool _dlcIsLoading;
    private int _dlcCount;
    private IEnumerable<DlcDto> _dlcs;
    
    //updates grid
    private RadzenDataGrid<UpdateDto> _updatesGrid;
    private bool _updatesIsLoading;
    private int _updatesCount;
    private IEnumerable<UpdateDto> _updates = default!;

    protected override async Task OnInitializedAsync()
    {
        if (TitleId is null) return;
        var title =  await TitledbService.GetTitleByApplicationId(TitleId);
        HtmlDescription = new MarkupString(title.Description.Text2Html()).Value;
        LibraryTitleDto = title;
        AgeRatingAgency = SettingsService.GetUserSettings().AgeRatingAgency;
    }
    
    private async Task LoadDlcData(LoadDataArgs args)
    {
        _dlcIsLoading = true;
        await Task.Yield();
        if (LibraryTitleDto?.Dlc is null || LibraryTitleDto.Dlc.Count == 0)
        {
            _dlcIsLoading = false;
            _dlcs = Array.Empty<DlcDto>();
            return;
        }

        var query = LibraryTitleDto?.Dlc.Select(t => new DlcDto
        {
            ApplicationId = t.ApplicationId,
            OtherApplicationId = t.OtherApplicationId,
            TitleName = t.TitleName,
            Size = t.Size,
            Version = t.Version,
            
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

        if (LibraryTitleDto?.Updates is null)
        {
            _updatesIsLoading = false;
            _updates = Array.Empty<UpdateDto>();
            return;
        }

        var update = LibraryTitleDto?.Updates.FirstOrDefault();
        if (update is null)
        {
            _updatesCount = 0;
            _updates = Array.Empty<UpdateDto>();
            _updatesIsLoading = false;
            return;
        }
        
        var query = LibraryTitleDto?.Versions?.Select(v => new UpdateDto
        {
            ApplicationId = update.ApplicationId,
            OtherApplicationId = update.OtherApplicationId,
            TitleName = update.TitleName,
            Date = v.VersionDate,
            Version = v.VersionNumber,
            ShortVersion = v.ShortVersionNumber,
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
}