﻿using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NsxLibraryManager.Data;
using Radzen;
using Radzen.Blazor;
using TitleModel = NsxLibraryManager.Models.Titledb.Title;
using System.Linq.Dynamic.Core;

namespace NsxLibraryManager.Pages;

public partial class SqlTitleDb : IDisposable
{
    [Inject]
    protected IJSRuntime JsRuntime { get; set; } = default!;
    
    [Inject]
    protected TitledbDbContext DbContext { get; set; } = default!;
    
    [Inject]
    protected DialogService DialogService { get; set; } = default!;

    private RadzenDataGrid<TitleModel> _grid = default!;
    private DataGridSettings? _settings;
    private static readonly string SettingsParamaterName = "SqlTitleDbGridSettings";


    private IEnumerable<TitleModel> _titles = default!;
    private IList<TitleModel> _selectedTitles = default!;
    private readonly IEnumerable<int> _pageSizeOptions = [25, 50, 100];
    private int _pageSize = 100;
    private int _count = 0;
    private bool _isLoading;
    private string _lastUpdated = "never";
    
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
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

        _titles = await Task.FromResult(
            query.
                Skip(args.Skip.Value).
                Take(args.Top.Value).
                ToList());

        _isLoading = false;
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadStateAsync();
            await Task.Delay(1);
            await _grid.Reload();
            StateHasChanged();    
        }
    }

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
        //await JsRuntime.InvokeVoidAsync("window.localStorage.setItem", SettingsParamaterName, JsonSerializer.Serialize(Settings));
    }
    
    private void LoadSettings(DataGridLoadSettingsEventArgs args)
    {
        if (Settings != null)
        {
            args.Settings = Settings;
        }
    }
    
    private async Task RefreshTitleDb()
    {
        var confirmationResult = await DialogService.Confirm(
            "Are you sure?", "Refresh TitleDb",
            new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" });
        if (confirmationResult is true)
        {
            DialogService.Close();
            await _grid.Reload();
            StateHasChanged();
        }
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
            _titles = default!;
        }
    }
}