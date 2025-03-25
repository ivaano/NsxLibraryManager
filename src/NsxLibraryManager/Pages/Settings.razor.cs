using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Extensions;
using NsxLibraryManager.Models;
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
    
    private RadzenDataGrid<LibraryLocationDto> _additionalLibraryPathsGrid = null!;
    private IEnumerable<LibraryLocationDto> _libraryLocationData = null!;
    private bool _isLoading;
    private int _count;
    private bool _newRecordInsertDisabled;
    
    private IEnumerable<CollectionDto> _collections = null!;


    private UserSettings _config = default!;
    private bool _databaseFieldDisabled = true;
    private ValidationResult? _validationResult;
    private RadzenUpload _uploadDd = default!;
    private string _homeDirKeysFilePath = string.Empty;
    private string _theme = string.Empty;

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
        { "DownloadSettings.Regions", string.Empty }
    };

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        LoadConfiguration();
        var configExists = bool.TryParse(Configuration.GetValue<string>("IsDefaultConfigCreated"), out _);
        if (configExists)
        {
            _databaseFieldDisabled = false;
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

        LoadConfiguration();
    }

    private void OnUploadKeysComplete(UploadCompleteEventArgs args)
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

        LoadConfiguration();
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

    private void ClearKeys()
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
            LoadConfiguration();
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

    private async void OnUpdateRow(LibraryLocationDto libraryLocationDto)
    {
    }
    
    private async void OnCreateRow(LibraryLocationDto libraryLocationDto)
    {
        CancelEdit(libraryLocationDto);

        ShowNotification(new NotificationMessage
        {
            Severity = NotificationSeverity.Error, 
            Summary = "Error Adding Collection", 
            Detail = "detail", 
            Duration = 4000
        });
    }
    
    private async Task LoadLibraryPathData()
    {
        _isLoading = true;
        var result = await TitleLibraryService.GetCollections();
        if (result.IsSuccess)
        {
            _collections = result.Value;
        }
        else
        {
            _collections = [];
        }

        _libraryLocationData = [];
        _count = 0;
        _isLoading = false;
    }
    
    private async Task InsertRow()
    {
        if (!_additionalLibraryPathsGrid.IsValid) return;
        var libraryLocation = new LibraryLocationDto();
        _newRecordInsertDisabled = true;
        await _additionalLibraryPathsGrid.InsertRow(libraryLocation);
    }
    
    private async Task InsertNewLibraryLocation(LibraryLocationDto row)
    {
        if (!_additionalLibraryPathsGrid.IsValid) return;
        _newRecordInsertDisabled = true;
        var libraryLocation = new LibraryLocationDto();
        await _additionalLibraryPathsGrid.InsertAfterRow(libraryLocation, row);
    }
    
    private async Task EditLibraryLocation(LibraryLocationDto libraryLocation)
    {
        if (!_additionalLibraryPathsGrid.IsValid) return;
        await _additionalLibraryPathsGrid.EditRow(libraryLocation);
    }
    
    private async Task DeleteLibraryLocation(LibraryLocationDto libraryLocation)
    {
        if (_libraryLocationData.Contains(libraryLocation))
        {
            /*
            var result = await TitleLibraryService.RemoveCollection(collectionDto);
            if (result.IsSuccess)
            {
                await LoadData();
            }
            else
            {
                CancelEdit(collectionDto);
                ShowNotification(
                    NotificationSeverity.Error, 
                    "Error Adding Collection", 
                    result.Error ?? "Unknown Error");
            }
            */
        }
        else
        {
            _additionalLibraryPathsGrid.CancelEditRow(libraryLocation);
        }

        await _additionalLibraryPathsGrid.Reload();
    }
    
    
    private void CancelEdit(LibraryLocationDto libraryLocation)
    {
        _newRecordInsertDisabled = false;
        _additionalLibraryPathsGrid.CancelEditRow(libraryLocation);
    }
    
    private async Task SaveNewLibraryLocation(LibraryLocationDto libraryLocation)
    {
        await _additionalLibraryPathsGrid.UpdateRow(libraryLocation);
    }

    private void ShowTooltip(string message, ElementReference elementReference, TooltipOptions options = null!) =>
        TooltipService.Open(elementReference, message, options);
}
