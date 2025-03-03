using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NsxLibraryManager.Pages.Components;
using NsxLibraryManager.Services;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.Shared.Enums;
using Radzen;
using Radzen.Blazor;

namespace NsxLibraryManager.Pages;

public partial class Library : IDisposable
{
    [Inject]
    protected IJSRuntime JsRuntime { get; set; } = null!;
    
    [Inject]
    protected ITitleLibraryService TitleLibraryService { get; set; } = null!;
    
    [Inject]
    protected DialogService DialogService { get; set; } = null!;
    
    [Inject]
    protected NotificationService NotificationService { get; set; } = null!;
    
    [Inject]
    private ISettingsService SettingsService { get; set; } = null!;
    
    [Inject]
    private FtpStateService FtpStateService { get; set; } = null!;
    
    //grid
    private DataGridSettings _settings = null!;
    private RadzenDataGrid<LibraryTitleDto> _grid = null!;
    private IEnumerable<LibraryTitleDto> _libraryTitles = null!;
    private bool _isLoading;
    private readonly IEnumerable<int> _pageSizeOptions = [10, 20, 30, 50, 100];
    private int _pageSize = 100;
    private int _count;
    private IEnumerable<string>? _selectedCategories;
    private IList<LibraryTitleDto>? _selectedTitles;
    private int _baseCount;
    private int _patchCount;
    private int _dlcCount;
    private string _libraryPath = string.Empty;
    private string _lastUpdated = string.Empty;
    private IList<int> _selectedTitleIds = new List<int>();
    private bool ActionsDisabled => _selectedTitles == null || _selectedTitles.Count == 0;
    private IEnumerable<string> _categories = [];
    private AgeRatingAgency AgeRatingAgency { get; set; }

    private const string SettingsParamaterName = "SqlLibraryGridSettings";
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
    
    private void ShowNotification(NotificationSeverity severity, string summary, string detail, int duration = 4000)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = severity, 
            Summary = summary, 
            Detail = detail, 
            Duration = duration
        });
    }
    
    protected override async Task OnInitializedAsync()
    {
        FtpStateService.OnTransferCompleted += HandleFtpStatusChanged;
        await base.OnInitializedAsync();
        await InitialLoad();
    }
    
    private void HandleFtpStatusChanged(FtpCompletedTransfer completedTransfer)
    {
        InvokeAsync(() =>
        {
            ShowNotification(
                NotificationSeverity.Success, 
                "Upload completed", 
                $"{completedTransfer.Filename} uploaded succesfully");
        });
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
    
    private async Task InitialLoad()
    {
        var lastUpdated = await TitleLibraryService.GetLastLibraryUpdateAsync();
        _libraryPath = lastUpdated?.LibraryPath ?? string.Empty;
        _baseCount = lastUpdated?.BaseTitleCount ?? 0;
        _patchCount = lastUpdated?.UpdateTitleCount ?? 0;
        _dlcCount = lastUpdated?.DlcTitleCount ?? 0;
        _lastUpdated = lastUpdated?.DateUpdated.ToString("MM/dd/yyyy h:mm tt") ?? "Never";
        AgeRatingAgency = SettingsService.GetUserSettings().AgeRatingAgency;

        var categoriesResult = await TitleLibraryService.GetCategoriesAsync();
        if (categoriesResult.IsSuccess)
        {
            _categories =categoriesResult.Value;
        } 
    }

    private async Task LoadData(LoadDataArgs args)
    {
        _isLoading = true;
        await Task.Yield();
        
        if (_selectedCategories is not null)
        {
            var filterList = args.Filters?.ToList() ?? [];
            var categoryFilters = _selectedCategories.Select(c => new FilterDescriptor()
            {
                Property = "Category", 
                FilterOperator = FilterOperator.Contains, 
                FilterValue = c
            }).ToList();
            filterList.AddRange(categoryFilters);
            args.Filters = filterList;
        }
        
        var titles = await TitleLibraryService.GetTitles(args);
        if (titles.IsSuccess)
        {
            _libraryTitles = titles.Value.Titles;
            _count = titles.Value.Count;
        }
        else
        {
            _libraryTitles = Array.Empty<LibraryTitleDto>();    
            _count = 0;
        }
        _isLoading = false;
    }
    
    private async Task OnSelectedCategoriesChange(object value)
    {
        await _grid.Reload();
    }
    
    private async Task OpenDetails(LibraryTitleDto title)
    {
        await DialogService.OpenAsync<Title>($"{title.TitleName}",
            new Dictionary<string, object>() { { "TitleId", title.ApplicationId } },
            new DialogOptions()
            {
                AutoFocusFirstElement = false,
                Width = "90%", 
                Height = "768px", 
                CloseDialogOnEsc = true, 
                CloseDialogOnOverlayClick = true, 
                Draggable = true, 
                Style = "background:var(--rz-base-900)"
            });
    }
    
    private async Task RefreshLibrary()
    {
        var confirmationResult = await DialogService.Confirm(
            "This action will add or remove titles that are not on the library.", "Refresh library?",
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
            "This action will clear the db and reload all your nsp/nsz files in your library path, Are you sure?", "Reload Library",
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
            await _grid.Reload();
        }
    }

    private async Task FtpSelected()
    {
        var result = await DialogService.OpenAsync<FtpSendDialog>($"Send by Ftp Multiple Titles",
            new Dictionary<string, object>() { { "SelectedTitles", _selectedTitles } },
            new DialogOptions()
            {
                CloseDialogOnEsc = true, 
                CloseDialogOnOverlayClick = true, 
                Draggable = true, 
                Style = "background:var(--rz-base-900)"
            });
    }

    private async Task EditSelected()
    {
        var result = await DialogService.OpenAsync<TitleMassEditDialog>($"Edit Multiple Titles",
            new Dictionary<string, object>() { { "SelectedTitles", _selectedTitles } },
            new DialogOptions()
            {
                CloseDialogOnEsc = true, 
                CloseDialogOnOverlayClick = true, 
                Draggable = true, 
                Style = "background:var(--rz-base-900)"
            });
        
        if (result is true) 
        {
            await _grid.Reload();
        }
    }

    private async Task EditRow(LibraryTitleDto title)
    {
        var result = await DialogService.OpenAsync<TitleEditDialog>($"Edit - {title.TitleName}",
            new Dictionary<string, object>() { { "TitleId", title.ApplicationId } },
            new DialogOptions()
            {
                Width = "90%", 
                Height = "768px", 
                CloseDialogOnEsc = true, 
                CloseDialogOnOverlayClick = true, 
                Draggable = true, 
                Style = "background:var(--rz-base-900)"
            });
        
        if (result is true) 
        {
            await _grid.Reload();
        }
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    private async Task OnUserRatingChange(int rating, LibraryTitleDto title)
    {
        title.UserRating = rating;
        var updatedTitleResult = await TitleLibraryService.UpdateLibraryTitleAsync(title);
        if (updatedTitleResult.IsSuccess)
        {
            ShowNotification(
                NotificationSeverity.Success, 
                "Rating Updated", 
                $"{title.TitleName} user rating updated");
        }
    }

    private async Task OnClickActions(RadzenSplitButtonItem item, string buttonName)
    {
        if (item is not null)
        {
            switch (item.Value)
            {
                case "FtpSelected":
                    await FtpSelected();
                    break;
                case "EditSelected":
                    await EditSelected();
                    break;
                case "ClearSelected":
                    _selectedTitles = null;
                    break;
            }
        }
    }
    

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _grid.Dispose();
            _libraryTitles = null!;
            _settings = null!;
            TitleLibraryService = null!;
        }
    }
}