using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using NsxLibraryManager.Contracts;
using NsxLibraryManager.Extensions;
using NsxLibraryManager.Models;
using NsxLibraryManager.Providers;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.Shared.Settings;
using Radzen;
using Radzen.Blazor;
using NsxLibraryManager.Utils;

namespace NsxLibraryManager.Pages;

public partial class Settings
{
    [Inject] private ISettingsService SettingsService { get; set; } = default!;
    [Inject] private TooltipService TooltipService { get; set; } = default!;
    [Inject] private IHostApplicationLifetime ApplicationLifetime { get; set; } = default!;
    [Inject] private IConfiguration Configuration { get; set; } = default!;
    [Inject] private IValidator<UserSettings> UserSettingsValidator { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private ThemeService ThemeService { get; set; } = default!;
    [Inject] private ITitleLibraryService TitleLibraryService { get; set; } = null!;
    [Inject] private DialogService DialogService { get; set; } = default!;

    
    private RadzenDataGrid<LibraryLocation> _additionalLibraryPathsGrid = null!;
    private IEnumerable<LibraryLocation> _libraryLocationData = null!;
    private bool _isLoading;
    private int _count;
    private bool _newRecordInsertDisabled;
    private readonly BooleanProvider _myBooleanProvider = new();
    private bool _settingsSaved = true;
    private IEnumerable<CollectionDto> _collections = null!;


    private UserSettings _config = default!;
    private bool _databaseFieldDisabled = true;
    private ValidationResult? _validationResult;
    private RadzenUpload _uploadDd = default!;
    private string _homeDirKeysFilePath = string.Empty;
    private string _theme = string.Empty;
    private string _libraryPathValidationMessage = string.Empty;
    

    private readonly Dictionary<string, string> _validationErrors = new()
    {
        { "TitleDatabase", string.Empty },
        { "ProdKeys", string.Empty },
        { "LibraryPath", string.Empty },
        { "BackupPath", string.Empty },
        { "DownloadSettings.TitleDbPath", string.Empty },
        { "DownloadSettings.TitleDbUrl", string.Empty },
        { "DownloadSettings.CnmtsUrl", string.Empty },
        { "DownloadSettings.VersionUrl", string.Empty },
        { "DownloadSettings.TimeoutInSeconds", string.Empty },
        { "DownloadSettings.Regions", string.Empty },
        { "LibraryReloadWebhookUrl", string.Empty },
        { "LibraryRefreshWebhookUrl", string.Empty }
    };

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadConfiguration();
        var configExists = bool.TryParse(Configuration.GetValue<string>("IsDefaultConfigCreated"), out _);
        if (configExists)
        {
            _databaseFieldDisabled = false;
            _settingsSaved = false;
            ShowNotification(new NotificationMessage
            {
                Severity = NotificationSeverity.Warning,
                Summary = "Default configuration created",
                Detail =
                    "A default config.json file has been created, set the correct paths for the application to work.",
                Duration = 60000
            });
        }
    }

    private async Task<bool> ValidateFields()
    {
        Array.ForEach(_validationErrors.Keys.ToArray(), key => _validationErrors[key] = string.Empty);
        _validationResult = await UserSettingsValidator.ValidateAsync(_config);
        if (!_validationResult.IsValid)
        {
            foreach (var failure in _validationResult.Errors)
            {
                _validationErrors[failure.PropertyName] =
                    _validationErrors.TryGetValue(failure.PropertyName, out var value)
                        ? $"{value} {failure.ErrorMessage}"
                        : failure.ErrorMessage;
                ShowNotification(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error, Summary = "Configuration Error",
                    Detail = failure.ErrorMessage, Duration = 3000
                });
            }
        }

        return _validationResult.IsValid;
    }

    private void ChangeTheme(string value)
    {
        ThemeService.SetTheme(value);
        _config.UiTheme = value;
    }

    private async Task SaveConfiguration()
    {
        var validateFields = await ValidateFields();
        if (!validateFields) return;

        SettingsService.SaveUserSettings(_config);
        ShowNotification(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Configuration Saved!",
            Detail = "Settings have been saved.",
            Duration = 4000
        });
        _settingsSaved = true;
    }

    private async Task LoadConfiguration()
    {
        _config = SettingsService.GetUserSettings();
        var homeUserFolder = PathHelper.HomeUserDir;
        if (homeUserFolder is not null)
        {
            _homeDirKeysFilePath = Path.Combine(homeUserFolder, ".switch").ToFullPath();
        }

        await LoadLibraryPathData();
    }

    private void OnUploadProgress(UploadProgressArgs args, string name)
    {
        if (args.Progress == 100)
        {
            foreach (var file in args.Files)
            {
                var log = $"Uploaded: {file.Name} / {file.Size} bytes";
            }
        }
    }

    private async Task OnUploadUserDataComplete(UploadCompleteEventArgs args)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        try
        {
            var uploadedFiles = args.JsonResponse.Deserialize<FileUploadResponse>(options);
            if (uploadedFiles is null) return;
            var importResult = await SettingsService.ImportUserData(uploadedFiles.FilePath);
            if (importResult.IsSuccess)
            {
                ShowNotification(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Import Success!",
                    Detail = $"{importResult.Value} records updated",
                    Duration = 20000,
                    CloseOnClick = true
                });
            }

            StateHasChanged();
        }
        catch (JsonException ex)
        {
            ShowNotification(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Upload Error",
                Detail = ex.Message,
                Duration = 20000,
                CloseOnClick = true
            });
        }

        await LoadConfiguration();
    }

    private async Task OnUploadKeysComplete(UploadCompleteEventArgs args)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        try
        {
            var uploadedFiles = args.JsonResponse.Deserialize<List<FileUploadResponse>>(options);
            if (uploadedFiles is null) return;
            foreach (var file in uploadedFiles)
            {
                ShowNotification(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = file.FileName,
                    Detail = file.Message,
                    Duration = 3000,
                    CloseOnClick = true
                });
            }

            StateHasChanged();
        }
        catch (JsonException ex)
        {
            ShowNotification(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Upload Error",
                Detail = ex.Message,
                Duration = 20000,
                CloseOnClick = true
            });
        }

        await LoadConfiguration();
    }

    private void OnUploadError(UploadErrorEventArgs args)
    {
        var errorDetail = string.Empty;
        var summary = string.Empty;
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var response = JsonSerializer.Deserialize<ErrorResponse>(args.Message, options);
            if (response is not null)
            {
                errorDetail = response.Errors.ListToString();
                summary = response.ErrorMessage;
            }
        }
        catch (JsonException e)
        {
            errorDetail = e.Message;
        }

        ShowNotification(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = summary,
            Detail = errorDetail,
            Duration = 20000,
            CloseOnClick = true
        });
    }

    private async Task ClearKeys()
    {
        if (SettingsService.RemoveCurrentKeys())
        {
            ShowNotification(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Success",
                Detail = "Keys have been removed.",
                Duration = 3000,
                CloseOnClick = true
            });
            await LoadConfiguration();
        }
    }

    private async Task ClearPersistentData()
    {
        var confirmationResult = await DialogService.Confirm("Are you sure?", "Remove persistent titles?",
            new ConfirmOptions() { OkButtonText = "Yes", CancelButtonText = "No" });
        if (confirmationResult == true)
        {
            await SettingsService.RemovePersistentData();
            ShowNotification(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Success",
                Detail = "Persisted titles have been removed.",
                Duration = 3000,
                CloseOnClick = true
            });
        }
        
    }

    private Task ReloadApp()
    {
        ApplicationLifetime.StopApplication();
        return Task.CompletedTask;
    }

    private void ShowNotification(NotificationMessage message)
    {
        NotificationService.Notify(message);
    }

    private void OnUpdateLibraryLocationRow(LibraryLocation libraryLocation)
    {
        _config.LibraryLocations.Add(libraryLocation);
        _newRecordInsertDisabled = false;
    }
    
    private void OnCreateLibraryLocationRow(LibraryLocation libraryLocation)
    {
        _config.LibraryLocations.Add(libraryLocation);
        _newRecordInsertDisabled = false;
    }
    
    private async Task LoadLibraryPathData()
    {
        _isLoading = true;

        var result = await TitleLibraryService.GetCollections();
        var noCollectionItem = new CollectionDto
        {
            Id = 0,
            Name = "No Collection"
        };
        
        var updatedCollections = new List<CollectionDto> { noCollectionItem };
        if (result.IsSuccess)
        {
            updatedCollections.AddRange(result.Value);
        }


        _collections = updatedCollections;

        _libraryLocationData = _config.LibraryLocations;
        _count = _libraryLocationData.Count();
        _isLoading = false;
    }
    
    private async Task InsertRow()
    {
        _settingsSaved = false;
        if (!_additionalLibraryPathsGrid.IsValid) return;
        var libraryLocation = new LibraryLocation();
        _newRecordInsertDisabled = true;
        await _additionalLibraryPathsGrid.InsertRow(libraryLocation);
    }
    
    private async Task InsertNewLibraryLocation(LibraryLocation row)
    {
        if (!_additionalLibraryPathsGrid.IsValid) return;
        _newRecordInsertDisabled = true;
        var libraryLocation = new LibraryLocation();
        await _additionalLibraryPathsGrid.InsertAfterRow(libraryLocation, row);
    }
    
    private async Task EditLibraryLocation(LibraryLocation libraryLocation)
    {
        if (!_additionalLibraryPathsGrid.IsValid) return;
        if (_libraryLocationData.Contains(libraryLocation))
        {
            _config.LibraryLocations.Remove(libraryLocation);
        }

        await _additionalLibraryPathsGrid.EditRow(libraryLocation);
    }
    
    private async Task DeleteLibraryLocation(LibraryLocation libraryLocation)
    {
        if (_libraryLocationData.Contains(libraryLocation))
        {
            _config.LibraryLocations.Remove(libraryLocation);
            await LoadLibraryPathData();
        }
        else
        {
            _additionalLibraryPathsGrid.CancelEditRow(libraryLocation);
        }

        await _additionalLibraryPathsGrid.Reload();
    }
    
    
    private void CancelEdit(LibraryLocation libraryLocation)
    {
        _newRecordInsertDisabled = false;
        _additionalLibraryPathsGrid.CancelEditRow(libraryLocation);
    }
    
    private async Task SaveNewLibraryLocation(LibraryLocation libraryLocation)
    {
        await _additionalLibraryPathsGrid.UpdateRow(libraryLocation);
    }
    
    private bool ValidatePath(string path)
    {
        
        var exists = _config.LibraryLocations.Any(item => item.Path == path);
        if (exists)
        {
            _libraryPathValidationMessage = "Path already exists in the configuration.";
            return false;
        }
        
        if (!Directory.Exists(path))
        {
            _libraryPathValidationMessage = "Directory doesn't exist.";
            return false;
        }

        return true;
    }

    private string GetCollectionName(int collectionId)
    {
        var collection = _collections.FirstOrDefault(x => x.Id == collectionId);
        return  collection?.Name ?? string.Empty;
    }

    private void ConfigChanged()
    {
        _settingsSaved = false;
    }
    
    private async Task OnBeforeInternalNavigation(LocationChangingContext context)
    {
        if (!_settingsSaved)
        {
            var confirmationResult = await DialogService.Confirm(
                $"Settings are not saved, do you really want to leave this page?", "Settings Not Saved",
                new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" });
            if (confirmationResult is not true)
            {
                context.PreventNavigation();
            }

        }

    }

    private void ShowTooltip(string message, ElementReference elementReference, TooltipOptions options = null!) =>
        TooltipService.Open(elementReference, message, options);
}
