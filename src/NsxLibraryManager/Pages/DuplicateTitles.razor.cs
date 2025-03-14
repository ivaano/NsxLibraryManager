using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Pages.Components;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.Shared.Enums;
using Radzen;
using Radzen.Blazor;

namespace NsxLibraryManager.Pages;

public partial class DuplicateTitles : ComponentBase
{
    [Inject] private ITitleLibraryService TitleLibraryService { get; set; } = default!; 
    [Inject] private ISettingsService SettingsService { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;


    private int selectedTabIndex = 0;
    private RadzenDataGrid<LibraryTitleDto> _duplicateBaseTitlesGrid = default!;
    private RadzenDataGrid<LibraryTitleDto> _duplicateUpdatesTitlesGrid = default!;
    private RadzenDataGrid<LibraryTitleDto> _duplicateDlcTitlesGrid = default!;
    private IEnumerable<LibraryTitleDto> _duplicateBaseTitles = default!;
    private IEnumerable<LibraryTitleDto> _duplicateUpdatesTitles = default!;
    private IEnumerable<LibraryTitleDto> _duplicateDlcTitles = default!;
    private bool _isLoading = false;
    private readonly IEnumerable<int> _pageSizeOptions = new[] { 25, 50, 100 };
    private int _countBaseTitles;
    private int _countUpdatesTitles;
    private int _countDlcTitles;
    private IList<LibraryTitleDto>? _selectedBaseTitles;
    private IList<LibraryTitleDto>? _selectedUpdatesTitles;
    private IList<LibraryTitleDto>? _selectedDlcTitles;
    private string _baseSelectTitlesText = "Select all duplicates";
    private string _updatesSelectTitlesText = "Select all duplicates";
    private string _dlcSelectTitlesText = "Select all duplicates";
    private string _backupPath = string.Empty;

    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await InitializeSettings();
    }
    
    private Task InitializeSettings()
    {
        var userSettings = SettingsService.GetUserSettings();
        _backupPath = userSettings.BackupPath;
        StateHasChanged();
        return Task.CompletedTask;
    }
    
    private async Task LoadData()
    {
        try
        {
            _isLoading = true;
            switch (selectedTabIndex)
            {
                case 0:
                {
                    var duplicateResult = await TitleLibraryService.GetDuplicateTitles(TitleContentType.Base);
                    if (duplicateResult.IsSuccess)
                    {
                        _countBaseTitles = duplicateResult.Value.Count;
                        _duplicateBaseTitles = duplicateResult.Value.Titles;
                    }
                    else
                    {
                    }

                    break;
                }
                case 1:
                {
                    var duplicateResult = await TitleLibraryService.GetDuplicateTitles(TitleContentType.Update);
                    if (duplicateResult.IsSuccess)
                    {
                        _countUpdatesTitles = duplicateResult.Value.Count;
                        _duplicateUpdatesTitles = duplicateResult.Value.Titles;
                    }
                    else
                    {
                    }

                    break;
                }
                case 2:
                {
                    var duplicateResult = await TitleLibraryService.GetDuplicateTitles(TitleContentType.DLC);
                    if (duplicateResult.IsSuccess)
                    {
                        _countDlcTitles = duplicateResult.Value.Count;
                        _duplicateDlcTitles = duplicateResult.Value.Titles;
                    }
                    else
                    {
                    }

                    break;
                }
            }
        }
        finally
        {
            _isLoading = false;
            SelectDuplicates();
            StateHasChanged();
        }
    }

    private void SelectDuplicates()
    {
        switch (selectedTabIndex)
        {
            case 0:
            {
                if (_baseSelectTitlesText == "Select all duplicates")
                {
                    _selectedBaseTitles = new List<LibraryTitleDto>();
                    foreach (var i in _duplicateBaseTitles)
                    {
                        if (i.IsDuplicate)
                        {
                            _selectedBaseTitles.Add(i);
                        }
                    }

                    _baseSelectTitlesText = "Clear selection";
                }
                else
                {
                    _baseSelectTitlesText = "Select all duplicates";
                    _selectedBaseTitles = new List<LibraryTitleDto>();
                }
                break;
            }
            case 1:
            {
                if (_updatesSelectTitlesText == "Select all duplicates")
                {
                    _selectedUpdatesTitles = new List<LibraryTitleDto>();
                    foreach (var i in _duplicateUpdatesTitles)
                    {
                        if (i.IsDuplicate)
                        {
                            _selectedUpdatesTitles.Add(i);
                        }
                    }
                    _updatesSelectTitlesText = "Clear selection";
                }
                else
                {
                    _updatesSelectTitlesText = "Select all duplicates";
                    _selectedUpdatesTitles = new List<LibraryTitleDto>();
                }
                break;
            }
            case 2:
            {
                if (_dlcSelectTitlesText == "Select all duplicates")
                {
                    _selectedDlcTitles = new List<LibraryTitleDto>();
                    foreach (var i in _duplicateDlcTitles)
                    {
                        if (i.IsDuplicate)
                        {
                            _selectedDlcTitles.Add(i);
                        }
                    }
                    _dlcSelectTitlesText = "Clear selection";
                }
                else
                {
                    _dlcSelectTitlesText = "Select all duplicates";
                    _selectedDlcTitles = new List<LibraryTitleDto>();
                }
                break;
            }
        }
    }

    private async void DeleteDuplicates()
    {
        switch (selectedTabIndex)
        {
            case 0:
            {
                if (_selectedBaseTitles is not null && _selectedBaseTitles.Count > 0)
                {
                    var confirmationResult = await DialogService.Confirm(
                        $"This action will delete {_selectedBaseTitles.Count} file(s), from your library do you want to continue?",
                        "Delete Files",
                        new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" });
                    if (confirmationResult is true)
                    {
                        DialogService.Close();
                        StateHasChanged();
                        var paramsDialog = new Dictionary<string, object>()
                            { { "selectedTitles", _selectedBaseTitles } };
                        var dialogOptions = new DialogOptions()
                            { ShowClose = false, CloseDialogOnOverlayClick = false, CloseDialogOnEsc = false };
                        await DialogService.OpenAsync<DeleteLibraryDuplicatesProgressDialog>(
                            "Deleting duplicates in library...", paramsDialog, dialogOptions);
                        await LoadData();
                        await _duplicateBaseTitlesGrid.Reload();
                    }
                }
                else
                {
                    await DialogService.Alert($"No titles selected, select 1 or more duplicate titles first.",
                        "No Titles Selected!");
                }

                break;
            }
            case 1:
            {
                if (_selectedUpdatesTitles is not null && _selectedUpdatesTitles.Count > 0)
                {
                    var confirmationResult = await DialogService.Confirm(
                        $"This action will delete {_selectedUpdatesTitles.Count} file(s), from your library do you want to continue?",
                        "Delete Files",
                        new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" });
                    if (confirmationResult is true)
                    {
                        DialogService.Close();
                        StateHasChanged();
                        var paramsDialog = new Dictionary<string, object>()
                            { { "selectedTitles", _selectedUpdatesTitles } };
                        var dialogOptions = new DialogOptions()
                            { ShowClose = false, CloseDialogOnOverlayClick = false, CloseDialogOnEsc = false };
                        await DialogService.OpenAsync<DeleteLibraryDuplicatesProgressDialog>(
                            "Deleting duplicates in library...", paramsDialog, dialogOptions);
                        await LoadData();
                        await _duplicateBaseTitlesGrid.Reload();
                    }
                }
                else
                {
                    await DialogService.Alert($"No titles selected, select 1 or more duplicate titles first.",
                        "No Titles Selected!");
                }
                break;
            }
            case 2:
            {
                if (_selectedDlcTitles is not null && _selectedDlcTitles.Count > 0)
                {
                    var confirmationResult = await DialogService.Confirm(
                        $"This action will delete {_selectedDlcTitles.Count} file(s), from your library do you want to continue?",
                        "Delete Files",
                        new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" });
                    if (confirmationResult is true)
                    {
                        DialogService.Close();
                        StateHasChanged();
                        var paramsDialog = new Dictionary<string, object>()
                            { { "selectedTitles", _selectedDlcTitles } };
                        var dialogOptions = new DialogOptions()
                            { ShowClose = false, CloseDialogOnOverlayClick = false, CloseDialogOnEsc = false };
                        await DialogService.OpenAsync<DeleteLibraryDuplicatesProgressDialog>(
                            "Deleting duplicates in library...", paramsDialog, dialogOptions);
                        await LoadData();
                        await _duplicateBaseTitlesGrid.Reload();
                    }
                }
                else
                {
                    await DialogService.Alert($"No titles selected, select 1 or more duplicate titles first.",
                        "No Titles Selected!");
                }
                break;
            }            
        }
    }
}