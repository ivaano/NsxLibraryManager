using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Providers;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.Shared.Enums;
using NsxLibraryManager.Shared.Mapping;
using NsxLibraryManager.Shared.Settings;
using Radzen;
using Radzen.Blazor;

namespace NsxLibraryManager.Pages;

public partial class BundleRenamer : ComponentBase
{
    private const Variant Variant = Radzen.Variant.Outlined;

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    [Inject] private TooltipService TooltipService { get; set; } = default!;

    [Inject] private IRenamerService RenamerService { get; set; } = default!;

    [Inject] private NotificationService NotificationService { get; set; } = default!;

    [Inject] private ISettingsService SettingsService { get; set; } = default!;
    
    [Inject] private DialogService DialogService { get; set; } = default!;
    
    private int selectedTabIndex = 0;
    
    private RadzenDataGrid<RenameTitleDto> _renameGrid = default!;
    private IEnumerable<RenameTitleDto> _renameTitles = default!;
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
    private bool _scanInputButtonDisabled = false;
    private bool _renameButtonDisabled = true;
    private string _inputPathDisplay = string.Empty;
    private string _outputPathDisplay = string.Empty;
    private readonly BooleanProvider _myBooleanProvider = new();


    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await ShowLoading();
        InitializeTemplateFields();
        await InitializeSettings();
        await SelectConfigurationTab();
    }
    
    private async Task SelectConfigurationTab()
    {
        if (_settings.InputPath != string.Empty) return;
        selectedTabIndex = 1;
        
        await DialogService.Alert(
            $"The Bundle Renamer lets you rename files using a template \n" +
            $"with placeholders that are replaced by file metadata or titledb data if available.\n" +
            $"The organization type for this renamer is based bundles, or titles related between them using ApplicationId and OtherApplicationId fields.\n" +
            $"Files do not need to be in your library, you can select any folder as input and output.\n" +
            $"Renamed files will not be added to your library.",
            "Welcome to Bundle Renamer", new AlertOptions { OkButtonText = "Got it" });
    }
    
    private async Task InitializeSettings()
    {
        _settings = await SettingsService.GetBundleRenamerSettings();
        _inputPathDisplay = _settings.InputPath;
        _outputPathDisplay = _settings.OutputBasePath;
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
    
    private async Task ResetGrid()
    {
        _renameButtonDisabled = true;
        _renameTitles = default!;
        StateHasChanged(); 
        await Task.Delay(1); 
    }
    
    private async Task LoadFiles()
    {
        try
        {
            isLoading = true;
            _scanInputButtonDisabled = true;

            await ResetGrid();
            _renameTitles = await RenamerService.GetFilesToRenameAsync(
                _settings.InputPath, RenameType.Bundle, _settings.Recursive);
            if (_renameTitles.Any())
            {
                _renameButtonDisabled = false;
            } 
                
        }
        finally
        {
            isLoading = false;
            _scanInputButtonDisabled = false;
            StateHasChanged();
        }
    }
    
    private void ShowTooltip(string message, ElementReference elementReference, TooltipOptions options = null!) => 
        TooltipService.Open(elementReference, message, options);
    
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
                "TitleName from TitleDb, otherwise used what is in the file",
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

    private async Task<bool> LoadDefaultTemplate()
    {
        var confirmationResult = await DialogService.Confirm(
            $"This action will replace current template do you want to continue?", "Default Template",
            new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" });

        if (confirmationResult is not true) return true;
        const string baseTemplate = "{TitleName}\\{TitleName} [{TitleId}][v{Version}].{Extension}";
        const string updateTemplate = "{AppName}\\{TitleName} [{TitleId}][v{Version}].{Extension}";
        const string dlcTemplate = "{AppName}\\DLC\\{TitleName} [{TitleId}][v{Version}].{Extension}";
        _templateFields[TitlePackageType.BundleBase].Value = baseTemplate.Replace("\\", Path.DirectorySeparatorChar.ToString());
        _templateFields[TitlePackageType.BundleUpdate].Value = updateTemplate.Replace("\\", Path.DirectorySeparatorChar.ToString());
        _templateFields[TitlePackageType.BundleDlc].Value = dlcTemplate.Replace("\\", Path.DirectorySeparatorChar.ToString());
        return true;
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
        await SettingsService.SaveBundleRenamerSettings(_settings);
        var notificationMessage = new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Configuration Saved",
            Duration = 4000
        };
        NotificationService.Notify(notificationMessage);
        await ResetGrid();
    }
    
    private async Task RenameFiles()
    {
        if (!_renameTitles.Any())
            return;
        _renameButtonDisabled = true;
        _scanInputButtonDisabled = true;
        var fileList = _renameTitles.ToList();
        var countFiles = fileList.Count(x => x.Error == false);
        
        var confirmationResult = await DialogService.Confirm(
            $"This action will rename {countFiles} file(s), do you want to continue?", "Rename Files",
            new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" });

        if (confirmationResult is true && fileList.Count > 0)
        {
            isLoading = true;
            await ResetGrid();
            _renameTitles = await RenamerService.RenameFilesAsync(fileList);
            if (_settings.DeleteEmptyFolders)
            {
                var deleteFoldersResult = await RenamerService.DeleteEmptyFoldersAsync(_settings.InputPath);
                if (!deleteFoldersResult)
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Some Folders Couldn't be Deleted",
                        Duration = 4000
                    });
                }
            }

            var stats = _renameTitles.ToList();
            var errors = stats.Count(x => x.Error);
            var success = stats.Count(x => x.RenamedSuccessfully);

            NotificationService.Notify(errors > 0
                ? new NotificationMessage
                {
                    Severity = NotificationSeverity.Error, Summary = "Rename process finished with som errors!",
                    Detail = $"{success} Files Renamed and {errors} error(s)", Duration = 6000
                }
                : new NotificationMessage
                {
                    Severity = NotificationSeverity.Success, Summary = "Rename process finished with success!",
                    Detail = $"{success} Files Renamed and {errors} error(s)", Duration = 4000
                });
            await _renameGrid.Reload();
            isLoading = false;
            _scanInputButtonDisabled = false;
        }

    }
    
}