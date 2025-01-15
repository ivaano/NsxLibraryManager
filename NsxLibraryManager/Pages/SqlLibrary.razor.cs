using System.Text.Json;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Data;
using Radzen;
using Radzen.Blazor;
using System.Linq.Dynamic.Core;
using System.Text;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Pages.Components;
using Microsoft.EntityFrameworkCore;
using NsxLibraryManager.Extensions;
using NsxLibraryManager.Models.Dto;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Utils;
using TitleModel = NsxLibraryManager.Models.NsxLibrary.Title;

namespace NsxLibraryManager.Pages;

public partial class SqlLibrary : IDisposable
{
    [Inject]
    protected IJSRuntime JsRuntime { get; set; } = null!;
    
    [Inject] 
    private ISettingsService SettingsService { get; set; } = null!;

    [Inject]
    protected NsxLibraryDbContext DbContext { get; set; } = null!;
    
    [Inject]
    protected DialogService DialogService { get; set; }
    
    private static readonly string SettingsParamaterName = "SqlLibraryGridSettings";

    private DataGridSettings _settings = default!;
    private IEnumerable<GridTitle> _libraryTitles;
    private RadzenDataGrid<GridTitle> _grid;

    private IEnumerable<string> _selectedCategories;
    private IEnumerable<string> _categories;
    private int _selectedTabIndex;
    private string _libraryPath = string.Empty;
    private bool _isLoading;
    private int _baseCount;
    private int _patchCount;
    private int _dlcCount;
    private int _count;
    private int _pageSize = 100;
    private string _lastUpdated;
    private List<FilterDescriptor> categoryFilters = [];

    
    private readonly IEnumerable<int> _pageSizeOptions = [10, 20, 30, 50, 100];
    private DataGridSettings? Settings 
    { 
        get => _settings;
        set
        {
            if (_settings != value)
            {
                _settings = value;
                InvokeAsync(SaveStateAsync);
            }
        }
    }
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await InitialLoad();
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadStateAsync();
            await Task.Delay(1);
            StateHasChanged();    
        }
    }
    
    private async Task InitialLoad()
    {
        var settings = SettingsService.GetUserSettings();
        _libraryPath = settings.LibraryPath;
        _categories = DbContext.Categories.Select(s => s.Name);
        await CalculateCounts();
    }

    private Task CalculateCounts()
    {
        try
        {
            _baseCount = DbContext.Titles
                .Count(x => x.ContentType == TitleContentType.Base);
            _patchCount = DbContext.Titles
                .Count(x => x.ContentType == TitleContentType.Update);
            _dlcCount = DbContext.Titles
                .Count(x => x.ContentType == TitleContentType.DLC);  
        }
        catch (Exception)
        {
            _baseCount = 0;
            _patchCount = 0;
            _dlcCount = 0;
        }

        return Task.CompletedTask;
    }
    
    private async Task OnSelectedCategoriesChange(object value)
    {
        categoryFilters.RemoveAll(f => f.Property == "CategoryNames");

        if (_selectedCategories?.Any() == true)
        {
            foreach (var selectedCategory in _selectedCategories)
            {
                categoryFilters.Add(new FilterDescriptor
                {
                    Property = "CategoryNames",
                    FilterValue = selectedCategory,
                    FilterOperator = FilterOperator.Contains
                });
            }
        }
        await _grid.Reload();
    }
    
    private async Task LoadData(LoadDataArgs args)
    {
        _isLoading = true;
        await Task.Yield();
        
        var queryT = DbContext.Titles.Include(x => x.Categories).AsQueryable();
        
        if (categoryFilters.Count > 0)
        {
            queryT = categoryFilters
                .Aggregate(queryT, (current, filter) => current.Where(t => t.Categories.Any(c => c.Name.ToLower().Contains(filter.FilterValue.ToString().ToLower()))));
        }
        
        var finalQuery = queryT.Select(t => new GridTitle
        {
            ApplicationId = t.ApplicationId,
            Categories = t.Categories,
            ContentType = t.ContentType,
            DlcCount = t.DlcCount,
            FileName = t.FileName,
            Id = t.Id,
            Intro = t.Intro,
            IsDemo = t.IsDemo,
            LastWriteTime = t.LastWriteTime,
            LatestVersion = t.LatestVersion,
            NsuId = t.NsuId,
            NumberOfPlayers = t.NumberOfPlayers,
            OtherApplicationId = t.OtherApplicationId,
            OwnedDlcs = t.OwnedDlcs,
            OwnedUpdates = t.OwnedUpdates,
            PackageType = t.PackageType,
            Publisher = t.Publisher,
            Region = t.Region,
            ReleaseDate = t.ReleaseDate,
            Size = t.Size,
            TitleName = t.TitleName,
            UpdatesCount = t.UpdatesCount,
            Version = t.Version,
        });
        
        //var finalQuery = queryT.Select(t => t.MapToLibraryTitleDto());
        
        if (args.Filters.Any())
        {
            finalQuery = finalQuery.Where(FilterBuilder.BuildFilterString(args.Filters));
        }

        if (!string.IsNullOrEmpty(args.OrderBy))
        {
            finalQuery = finalQuery.OrderBy(args.OrderBy);
        }

        _count = finalQuery.Count();

        _libraryTitles = await finalQuery.
            Skip(args.Skip.Value).
            Take(args.Top.Value).ToListAsync();
        
        _isLoading = false;
    }
    
    
    private async Task LoadStateAsync()
    {
        var result = await JsRuntime.InvokeAsync<string>("window.localStorage.getItem", SettingsParamaterName);
        if (!string.IsNullOrEmpty(result))
        {
            _settings = JsonSerializer.Deserialize<DataGridSettings>(result);
            if (_settings is { PageSize: not null })
            {
                _pageSize = _settings.PageSize.Value;
            }
        }
        await Task.CompletedTask;
    }
    
    private async Task SaveStateAsync()
    {
        await JsRuntime.InvokeVoidAsync("window.localStorage.setItem", SettingsParamaterName, 
            JsonSerializer.Serialize<DataGridSettings>(Settings));
        
        await Task.CompletedTask;
    }
    

    private void TabOnChange(int index)
    {
         
    }

    private async Task RefreshLibrary()
    {
        var confirmationResult = await DialogService.Confirm(
            "This refresh the library", "Refresh library?",
            new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" });
        if (confirmationResult is true)
        {
            DialogService.Close();
            StateHasChanged();
            var paramsDialog = new Dictionary<string, object>();
            var dialogOptions = new DialogOptions()
                { ShowClose = false, CloseDialogOnOverlayClick = false, CloseDialogOnEsc = false };
            await DialogService.OpenAsync<RefreshLibraryProgressDialog>(
                "Refreshing library...", paramsDialog, dialogOptions);

            await InitialLoad();
            await _grid.Reload();
        }
    }

    private async Task ReloadLibrary()
    {
        var confirmationResult = await DialogService.Confirm(
            "This will reload all titles to the db Are you sure?", "Reload Library",
            new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" });
        if (confirmationResult is true)
        {
            DialogService.Close();
            StateHasChanged();
            var paramsDialog = new Dictionary<string, object>();
            var dialogOptions = new DialogOptions()
                { ShowClose = false, CloseDialogOnOverlayClick = false, CloseDialogOnEsc = false };
            await DialogService.OpenAsync<ReloadLibraryProgressDialog>(
                "Reloading library...", paramsDialog, dialogOptions);

            await InitialLoad();
            switch (_selectedTabIndex)
            {
                case 0:
                    await _grid.Reload();
                    break;
                case 1:
                    //await _dlcGrid.Reload();
                    break;
                case 2:
                    //await _updatesGrid.Reload();
                    break;
            }
        }
    }

    private async Task OpenDetails(GridTitle title)
    {
        await DialogService.OpenAsync<Title>($"{title.TitleName}",
            new Dictionary<string, object>() { { "TitleId", title.ApplicationId } },
            new DialogOptions() { Width = "80%", Height = "768px", CloseDialogOnEsc = true, CloseDialogOnOverlayClick = true, Draggable = true, Style = "background:var(--rz-base-900)"});
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);

    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _grid.Dispose();
        }
    }
}