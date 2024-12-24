using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Data;
using NsxLibraryManager.Models;
using TitleModel = NsxLibraryManager.Models.Titledb.Title;

namespace NsxLibraryManager.Pages;

public partial class SqlTitleDb : IDisposable
{
    [Inject]
    protected IJSRuntime JsRuntime { get; set; } = default!;
    
    [Inject]
    protected TitledbDbContext DbContext { get; set; } = default!;

    
    private IQueryable<TitleModel> _titles = default!;
    private IList<TitleModel> _selectedTitles = default!;
    private readonly IEnumerable<int> _pageSizeOptions = [25, 50, 100];
    private int _pageSize = 100;

    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        _titles = DbContext.Titles.AsQueryable().Where(t => t.TitleName != null).OrderByDescending(t => t.ReleaseDate);
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);

    }
    
    protected virtual void Dispose(bool disposing)
    {
        /*
        if (disposing)
        {
            _grid.Dispose();
            _regionTitles = default!;
        }*/
    }
}