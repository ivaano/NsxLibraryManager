﻿using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Mapping;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Settings;
using NsxLibraryManager.Services.Interface;
using Radzen;
using Radzen.Blazor;

namespace NsxLibraryManager.Pages;

public partial class BundleRenamer : ComponentBase
{
    private const Variant Variant = Radzen.Variant.Outlined;

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    [Inject] private TooltipService TooltipService { get; set; } = default!;

    [Inject] private ISqlRenamerService RenamerService { get; set; } = default!;

    [Inject] private NotificationService NotificationService { get; set; } = default!;

    [Inject] private ISettingsService SettingsService { get; set; } = default!;
    
    [Inject] private DialogService DialogService { get; set; } = default!;
    
    private int selectedTabIndex = 0;
    
    private RadzenDataGrid<RenameTitle> _renameGrid = default!;
    private IEnumerable<RenameTitle> _renameTitles = default!;
    private bool isLoading = false;
    private BundleRenamerSettings _settings = default!;
    private readonly IEnumerable<int> _pageSizeOptions = new[] { 25, 50, 100 };    
    private readonly Dictionary<TemplateField, string> _templateFieldMappings =
        RenamerTemplateFields.TemplateFieldMappings;
    private TitlePackageType _currentTitlePackage = TitlePackageType.None;
    private readonly Dictionary<TitlePackageType, TemplateFieldInfo> _templateFields = new();
    private string _sampleBefore = string.Empty;
    private string _sampleAfter = string.Empty;
    private bool _fragmentButtonDisabled = true;
    private string _sampleResultLabel = "Sample Result";
    private readonly Dictionary<string, string> _validationErrors = new()
    {
        { "InputPath", string.Empty },
        { "OutputBasePath", string.Empty }
    };
    
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await ShowLoading();
        InitializeTemplateFields();
        await InitializeSettings();

        await SelectConfigurationTab();
    }
    
    private Task SelectConfigurationTab()
    {
        if (_settings.InputPath != string.Empty) return Task.CompletedTask;
        selectedTabIndex = 1;
        return Task.CompletedTask;

    }
    
    private async Task InitializeSettings()
    {
        _settings = await SettingsService.GetBundleRenamerSettings();
        _ = await RenamerService.LoadRenamerSettingsAsync(_settings);

        _templateFields[TitlePackageType.BundleBase].Value = _settings.BundleBase;
        _templateFields[TitlePackageType.BundleDlc].Value = _settings.BundleDlc;
        _templateFields[TitlePackageType.BundleUpdate].Value = _settings.BundleUpdate;
    }
    
    private async Task ShowLoading()
    {
        isLoading = true;
        await Task.Yield();
        isLoading = false;
    }
    
    private void InitializeTemplateFields()
    {
        var textBoxTypes = new[]
        {
            TitlePackageType.BundleBase,
            TitlePackageType.BundleDlc,
            TitlePackageType.BundleUpdate,
        };

        foreach (var textBoxType in textBoxTypes)
        {
            _templateFields[textBoxType] = new TemplateFieldInfo
            {
                FieldType = textBoxType,
                Value = string.Empty
            };
        }
    }
    
    private async Task LoadFiles()
    {
        try
        {
            _renameTitles = default!;
            isLoading = true;
            StateHasChanged(); 
            await Task.Delay(1); // Ensure UI updates before heavy operation
            _renameTitles = await RenamerService.GetFilesToRenameAsync(
                _settings.InputPath, RenameType.Bundle, _settings.Recursive);
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }
    
    private void ShowTooltip(TemplateField templateField, ElementReference elementReference)
    {
        var options = new TooltipOptions()
        {
            Delay = 300,
            Duration = 5000,
            Position = TooltipPosition.Top,
            CloseTooltipOnDocumentClick = true
        };

        if (templateField is TemplateField.Extension or TemplateField.AppName or TemplateField.PatchId
            or TemplateField.PatchCount)
            options.Position = TooltipPosition.Left;

        var content = templateField switch
        {
            TemplateField.BasePath => "Path from the output field Base Path",
            TemplateField.TitleName =>
                "The first name among the list of declared titles, or the one coming from titledb",
            TemplateField.TitleId   => "The title id eg [0100F2200C984000]",
            TemplateField.Version   => "The version of the title eg [65536]",
            TemplateField.Extension => "The extension of the file based on its contents eg .nsp",
            TemplateField.AppName =>
                "Title name of the corresponding Application defined in OtherApplicationId, useful in updates and dlc to see the Application they belong to",
            TemplateField.PatchId =>
                "If content is an Application, this value is equal to the id of the corresponding Patch content, otherwise empty",
            TemplateField.PatchCount =>
                "Number of Patches for this title on TitleDb, otherwise empty",
            TemplateField.DlcCount =>
                "Number of DLC for this title on TitleDb, otherwise empty",
            TemplateField.Region =>
                "Title Region from Titledb eg [US]",
            _ => string.Empty
        };

        TooltipService.Open(elementReference, content, options);
    }
    
    private async Task OnTemplateFieldInput(TitlePackageType type, string? value)
    {
        if (value is not null)
        {
            _templateFields[type].Value = value;
            await TemplateTextboxUpdateNew(type);
            await UpdateSampleBox(_currentTitlePackage, value);
        }
    }
    
    private async Task OnTemplateFieldClick(TitlePackageType type, MouseEventArgs args)
    {
        await TemplateTextboxUpdateNew(type);
        await UpdateSampleBox(_currentTitlePackage, _templateFields[_currentTitlePackage].Value);
    }
    
    private void UpdateTemplateFieldRecord(TemplateField templateField, TitlePackageType type)
    {
        var templateFieldInfo = _templateFields[_currentTitlePackage];
        var templateFieldValue = _templateFieldMappings[templateField];

        templateFieldInfo.Value = templateFieldInfo.CursorPosition > 0
            ? templateFieldInfo.Value.Insert(templateFieldInfo.CursorPosition, templateFieldValue)
            : templateFieldValue;

        templateFieldInfo.CursorPosition += templateFieldValue.Length;

        Task.Run(() => UpdateSampleBox(_currentTitlePackage, templateFieldInfo.Value));
    }
    
    private async Task UpdateSampleBox(TitlePackageType type, string templateValue)
    {
        _sampleBefore = $"{_settings.InputPath}{Path.DirectorySeparatorChar}lucas-game.nsp";
        var basePathIncluded = $"{{BasePath}}{templateValue}";
        _sampleAfter = await RenamerService.CalculateSampleFileName(basePathIncluded, type, "inputFile.nsp", RenameType.Bundle);
    }
    
    private async Task TemplateTextboxUpdateNew(TitlePackageType type)
    {
        _currentTitlePackage = type;
        var labelParts = Regex.Split(_currentTitlePackage.ToString(), "(?<!^)(?=[A-Z])");
        _sampleResultLabel = string.Join(" ", labelParts);
        if (type == TitlePackageType.None)
        {
            _sampleResultLabel = "Sample Result";
            _fragmentButtonDisabled = true;
            return;
        }

        _fragmentButtonDisabled = false;
        _templateFields[type].CursorPosition =
            await JsRuntime.InvokeAsync<int>("getCursorLocation", type.ToString(), " {0}");
    }
    
    private async Task TemplateFragmentClick(TemplateField templateFieldType, MouseEventArgs args)
    {
        if (_currentTitlePackage != TitlePackageType.None)
        {
            UpdateTemplateFieldRecord(templateFieldType, _currentTitlePackage);
            await JsRuntime.InvokeVoidAsync("setFocus", _currentTitlePackage.ToString(), " {0}");
        }
    }

    private async Task<bool> ValidateConfiguration()
    {
        Array.ForEach(_validationErrors.Keys.ToArray(), key => _validationErrors[key] = string.Empty);
        var validationResult = await RenamerService.ValidateRenamerSettingsAsync(_settings);

        var notificationMessage = new NotificationMessage
        {
            Severity = validationResult.IsValid ? NotificationSeverity.Success : NotificationSeverity.Error,
            Summary = validationResult.IsValid ? "Success Validation" : "Validation Failed",
            Detail = validationResult.IsValid ? "All good!" : "Please check the fields!",
            Duration = 4000
        };
        
        if (!validationResult.IsValid)
        {
            foreach (var failure in validationResult.Errors)
            {
                _validationErrors[failure.PropertyName] =
                    _validationErrors.TryGetValue(failure.PropertyName, out var value)
                        ? $"{value} {failure.ErrorMessage}"
                        : failure.ErrorMessage;
            }
        }
        
        _settings.BundleBase = _templateFields[TitlePackageType.BundleBase].Value;
        _settings.BundleDlc = _templateFields[TitlePackageType.BundleDlc].Value;
        _settings.BundleUpdate = _templateFields[TitlePackageType.BundleUpdate].Value;

        NotificationService.Notify(notificationMessage);
        return validationResult.IsValid;
    }

    private async Task SaveConfiguration()
    {
        var isValid = await ValidateConfiguration();
        if (!isValid)
            return;
        var savedSettings = await SettingsService.SaveBundleRenamerSettings(_settings);
        var notificationMessage = new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Configuration Saved",
            Duration = 4000
        };
        NotificationService.Notify(notificationMessage);
    }
    
    private async Task RenameFiles()
    {
        if (!_renameTitles.Any())
            return;
        var fileList = _renameTitles.ToList();
        var countFiles = fileList.Count(x => x.Error == false);
        
        var confirmationResult = await DialogService.Confirm(
            $"This action will rename {countFiles} file(s), do you want to continue?", "Rename Files",
            new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" });

        if (confirmationResult is true && fileList.Any())
        {
            isLoading = true;
            _renameTitles = await RenamerService.RenameFilesAsync(fileList);
            var stats = _renameTitles.ToList();
            var errors = stats.Count(x => x.Error);
            var success = stats.Count(x => x.RenamedSuccessfully);   

            NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Warning, Summary = "Rename Process Finished!", Detail = $"{success} Files Renamed and {errors} error(s)", Duration = 4000 });
            await _renameGrid.Reload();
            isLoading = false;
        }

    }
    
}