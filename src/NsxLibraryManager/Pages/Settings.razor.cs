using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Extensions;
using NsxLibraryManager.Models;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Settings;
using Radzen;
using Radzen.Blazor;
using NsxLibraryManager.Utils;

namespace NsxLibraryManager.Pages;

public partial class Settings
{
    [Inject] private ISettingsService SettingsService { get; set; } = default!;
    [Inject] private TooltipService TooltipService { get; set; } = default!;
    [Inject] private IHostApplicationLifetime ApplicationLifetime  { get; set; } = default!;
    [Inject] private IConfiguration Configuration { get; set; } = default!;
    [Inject] private IValidator<UserSettings> UserSettingsValidator { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private ThemeService ThemeService { get; set; } = default!;


    private UserSettings _config = default!;
    private bool _databaseFieldDisabled = true;
    private ValidationResult? _validationResult;
    private RadzenUpload _uploadDd = default!;
    private string _homeDirKeysFilePath = string.Empty;
    private string _theme = string.Empty;
    
    private readonly Dictionary<string, string> _validationErrors = new()
    {
        { "TitleDatabase", string.Empty },
        { "ProdKeys", string.Empty},
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
                Detail = "A default config.json file has been created, set the correct paths for the application to work.", 
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
                ShowNotification(new NotificationMessage { Severity = NotificationSeverity.Error, Summary = "Configuration Error", Detail = failure.ErrorMessage, Duration = 3000 });
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
    
    private void LoadConfiguration()
    {
        _config = SettingsService.GetUserSettings();
        var homeUserFolder = PathHelper.HomeUserDir;
        if (homeUserFolder is not null)
        {
            _homeDirKeysFilePath = Path.Combine(homeUserFolder, ".switch").ToFullPath();
        }
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
    
    private void ShowTooltip(string message, ElementReference elementReference, TooltipOptions options = null!) => 
        TooltipService.Open(elementReference, message, options);
}



public class Region
{
    public string Name { get; set; } = string.Empty;
}