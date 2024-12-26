using System.Text.Json;
using Microsoft.JSInterop;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Data;
using Radzen;
using Radzen.Blazor;
using System.Linq.Dynamic.Core;

using TitleModel = NsxLibraryManager.Models.NsxLibrary.Title;

namespace NsxLibraryManager.Pages;

public partial class SqlLibrary : IDisposable
{
    [Inject]
    protected IJSRuntime JsRuntime { get; set; } = default!;
    
    [Inject]
    protected ITitleLibraryService TitleLibraryService { get; set; }
    
    [Inject]
    protected NsxLibraryDbContext DbContext { get; set; } = default!;
    
    [Inject]
    protected DialogService DialogService { get; set; }
    
    private static readonly string SettingsParamaterName = "SqlTitleDbGridSettings";

    [CanBeNull] private DataGridSettings _settings;
    private IEnumerable<TitleModel> _libraryTitles;
    private RadzenDataGrid<TitleModel> _grid;

    private int _selectedTabIndex;
    private string _libraryPath = string.Empty;
    private bool _isLoading;
    private int _baseCount;
    private int _patchCount;
    private int _dlcCount;
    private int _count;
    private int _pageSize = 100;
    private string _lastUpdated;
    
    private readonly IEnumerable<int> _pageSizeOptions = new[] { 10, 20, 30, 50, 100 };
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
    
    private Task InitialLoad()
    {
        _isLoading = true;
        _libraryPath = TitleLibraryService.GetLibraryPath();
        return Task.CompletedTask;
    }
    
    private async Task LoadData(LoadDataArgs args)
    {
        _isLoading = true;
        await Task.Yield();

        var query = DbContext.Titles.AsQueryable();
        
        if (!string.IsNullOrEmpty(args.Filter))
        {
            query = query.Where(args.Filter);
        }

        if (!string.IsNullOrEmpty(args.OrderBy))
        {
            query = query.OrderBy(args.OrderBy);
        }

        _count = query.Count();

        _libraryTitles = await Task.FromResult(
            query.
                Skip(args.Skip.Value).
                Take(args.Top.Value).
                ToList());

        _isLoading = false;
    }
    
    private async Task SaveStateAsync()
    {
        await JsRuntime.InvokeVoidAsync("window.localStorage.setItem", SettingsParamaterName, 
            JsonSerializer.Serialize<DataGridSettings>(Settings));
        
        await Task.CompletedTask;
    }
    

    
    private void LoadSettings(DataGridLoadSettingsEventArgs args)
    {
        if (Settings != null)
        {
            args.Settings = Settings;
        }
    }

    private void TabOnChange(int index)
    {
         
    }

    private async Task RefreshLibrary()
    {
    }

    private async Task ReloadLibrary()
    {
    }
    
    public async Task OpenDetails(TitleModel title)
    {
        await DialogService.OpenAsync<Title>($"{title.TitleName}",
            new Dictionary<string, object>() { { "TitleId", title.ApplicationId } },
            new DialogOptions() { Width = "80%", Height = "768px", CloseDialogOnEsc = true, CloseDialogOnOverlayClick = true, Draggable = true });
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