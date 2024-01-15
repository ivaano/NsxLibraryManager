﻿using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using NsxLibraryManager.Core.Settings;
using Radzen;

namespace NsxLibraryManager.Pages;

public partial class Settings
{
    
    [Inject] private IOptionsSnapshot<AppSettings> AppSettings { get; set; } = default!;
    [Inject] private IHostApplicationLifetime ApplicationLifetime  { get; set; } = default!;
    [Inject] private IConfiguration Configuration { get; set; } = default!;
    [Inject] private IValidator<AppSettings> ConfigValidator { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;

    private IEnumerable<string> _regionsValue = new string[] { "US" };
    private IEnumerable<Region> _regions = new List<Region>() { new() { Name = "US" } };
    private AppSettings _config = default!;
    private bool _databaseFieldDisabled = true;
    private ValidationResult? _validationResult;

    private readonly Dictionary<string, string> _validationErrors = new()
    {
        { "TitleDatabase", string.Empty },
        { "ProdKeys", string.Empty},
        { "LibraryPath", string.Empty },
        { "DownloadSettings.TitleDbPath", string.Empty },
        { "DownloadSettings.RegionUrl", string.Empty },
        { "DownloadSettings.CnmtsUrl", string.Empty },
        { "DownloadSettings.VersionsUrl", string.Empty },
        { "DownloadSettings.TimeoutInSeconds", string.Empty },
        { "DownloadSettings.Regions", string.Empty }
    };
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadConfiguration();
        bool.TryParse(Configuration.GetValue<string>("IsDefaultConfigCreated"), out var isDefaultConfigCreated);
        if (isDefaultConfigCreated)
        {
            _databaseFieldDisabled = false;
            ShowNotification(new NotificationMessage { Severity = NotificationSeverity.Warning, Summary = "Default configuration created", Detail = "A default config.json file has been created, setup the correct paths and restart the application.", Duration = 60000 });
        }

        await ValidateFields();
    }

    private async Task<bool> ValidateFields()
    {
        Array.ForEach(_validationErrors.Keys.ToArray(), key => _validationErrors[key] = string.Empty);
        _validationResult = await ConfigValidator.ValidateAsync(_config);
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
    
    private async Task SaveConfiguration()
    {
        var validateFields =await ValidateFields();
        if (!validateFields) return;
        
        var jsonWriteOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        jsonWriteOptions.Converters.Add(new JsonStringEnumConverter());

        var sectionName = new Dictionary<string, AppSettings>
        {
            { AppConstants.AppSettingsSectionName, _config }
        };

        var newJson = JsonSerializer.Serialize(sectionName, jsonWriteOptions);
        var appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppConstants.ConfigFileName);
        await File.WriteAllTextAsync(appSettingsPath, newJson);

        var configurationRoot = (IConfigurationRoot)Configuration;
        configurationRoot.Reload();
        ShowNotification(new NotificationMessage { Severity = NotificationSeverity.Success, Summary = "Configuration Saved!", Detail = "Settings have been saved to config.json.", Duration = 4000 });
    }
    
    private Task LoadConfiguration()
    {
        _config = AppSettings.Value;
        return Task.CompletedTask;
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
}



public class Region
{
    public string Name { get; set; } = string.Empty;
}