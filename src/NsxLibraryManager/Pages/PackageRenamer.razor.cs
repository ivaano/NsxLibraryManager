using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Providers;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.Shared.Enums;
using NsxLibraryManager.Shared.Mapping;
using NsxLibraryManager.Shared.Settings;
using Radzen;
using Radzen.Blazor;
using IRenamerService = NsxLibraryManager.Services.Interface.IRenamerService;

namespace NsxLibraryManager.Pages;

public partial class PackageRenamer
{
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    [Inject] private TooltipService TooltipService { get; set; } = default!;

    [Inject] private IRenamerService RenamerService { get; set; } = default!;

    [Inject] private NotificationService NotificationService { get; set; } = default!;

    [Inject] private ISettingsService SettingsService { get; set; } = default!;
    
    [Inject] private DialogService DialogService { get; set; } = default!;
    
    private const Variant Variant = Radzen.Variant.Outlined;
    private string _sampleBefore = string.Empty;
    private string _sampleAfter = string.Empty;
    private string _sampleResultLabel = "Sample Result";
    private TitlePackageType _currentTitlePackage = TitlePackageType.None;
    private readonly Dictionary<TitlePackageType, TemplateFieldInfo> _templateFields = new();
    private int selectedTabIndex = 0;
    private readonly Dictionary<TemplateField, string> _templateFieldMappings = RenamerTemplateFields.TemplateFieldMappings;
    private readonly BooleanProvider _myBooleanProvider = new();

    private readonly Dictionary<string, string> _validationErrors = new()
    {
        { "InputPath", string.Empty },
        { "OutputBasePath", string.Empty }
    };

    private bool _fragmentButtonDisabled = true;
    private PackageRenamerSettings _settings = default!;
    private IEnumerable<RenameTitleDto> _renameTitles = default!;
    private RadzenDataGrid<RenameTitleDto> _renameGrid = default!;
    private readonly IEnumerable<int> _pageSizeOptions = new[] { 25, 50, 100 };    
    private bool _isLoading = false;
    private bool _scanInputButtonDisabled = false;
    private bool _renameButtonDisabled = true;
    private string _inputPathDisplay = string.Empty;
    private string _outputPathDisplay = string.Empty;
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await ShowLoading();
        InitializeTemplateFields();
        await InitializeSettings();
        await SelectConfigurationTab();
    }

    private async Task ShowLoading()
    {
        _isLoading = true;
        await Task.Yield();
        _isLoading = false;
    }
    
    private async Task SelectConfigurationTab()
    {
        if (_settings.InputPath != string.Empty) return;
        selectedTabIndex = 1;
        
        await DialogService.Alert(
            $"The Package Renamer lets you rename files using a template \n" +
            $"with placeholders that are replaced by file metadata or titledb data if available.\n" +
            $"The organization type for this renamer is based on the package type (NSP, NSZ, XCI, XCZ)\n" +
            $"Files do not need to be in your library, you can select any folder as input and output.\n" +
            $"Renamed files will not be added to your library.",
            "Welcome to Package Renamer", new AlertOptions { OkButtonText = "Got it" });
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
            _isLoading = true;
            _scanInputButtonDisabled = true;

            await ResetGrid();
            _renameTitles = await RenamerService.GetFilesToRenameAsync(
                _settings.InputPath, RenameType.PackageType, _settings.Recursive);
            if (_renameTitles.Any())
            {
                _renameButtonDisabled = false;
            } 
                
        }
        finally
        {
            _isLoading = false;
            _scanInputButtonDisabled = false;
            StateHasChanged();
        }
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
            _isLoading = true;
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
            _isLoading = false;
            _scanInputButtonDisabled = false;
        }

    }

    private async Task InitializeSettings()
    {
        _settings = await SettingsService.GetPackageRenamerSettings();
        _inputPathDisplay = _settings.InputPath;
        _outputPathDisplay = _settings.OutputBasePath;
        _ = await RenamerService.LoadRenamerSettingsAsync(_settings);

        _templateFields[TitlePackageType.NspBase].Value = _settings.NspBasePath;
        _templateFields[TitlePackageType.NspDlc].Value = _settings.NspDlcPath;
        _templateFields[TitlePackageType.NspUpdate].Value = _settings.NspUpdatePath;
        _templateFields[TitlePackageType.NszBase].Value = _settings.NszBasePath;
        _templateFields[TitlePackageType.NszDlc].Value = _settings.NszDlcPath;
        _templateFields[TitlePackageType.NszUpdate].Value = _settings.NszUpdatePath;
        _templateFields[TitlePackageType.XciBase].Value = _settings.XciBasePath;
        _templateFields[TitlePackageType.XciDlc].Value = _settings.XciDlcPath;
        _templateFields[TitlePackageType.XciUpdate].Value = _settings.XciUpdatePath;
        _templateFields[TitlePackageType.XczBase].Value = _settings.XczBasePath;
        _templateFields[TitlePackageType.XczDlc].Value = _settings.XczDlcPath;
        _templateFields[TitlePackageType.XczUpdate].Value = _settings.XczUpdatePath;
    }

    private void InitializeTemplateFields()
    {
        var textBoxTypes = new[]
        {
            TitlePackageType.NspBase,
            TitlePackageType.NspDlc,
            TitlePackageType.NspUpdate,
            TitlePackageType.NszBase,
            TitlePackageType.NszDlc,
            TitlePackageType.NszUpdate,
            TitlePackageType.XciBase,
            TitlePackageType.XciDlc,
            TitlePackageType.XciUpdate,
            TitlePackageType.XczBase,
            TitlePackageType.XczDlc,
            TitlePackageType.XczUpdate
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

    private async Task UpdateSampleBox(TitlePackageType type, string templateValue)
    {
        _sampleBefore = $"c:\\dump\\lucas-game.nsp";
        _sampleAfter = await RenamerService.CalculateSampleFileName(templateValue, type, "inputFile.nsp", RenameType.PackageType);
    }

    private async Task TemplateFragmentClick(TemplateField templateFieldType, MouseEventArgs args)
    {
        if (_currentTitlePackage != TitlePackageType.None)
        {
            UpdateTemplateFieldRecord(templateFieldType, _currentTitlePackage);
            await JsRuntime.InvokeVoidAsync("setFocus", _currentTitlePackage.ToString(), " {0}");
        }
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


    private async Task OnTemplateFieldClick(TitlePackageType type, MouseEventArgs args)
    {
        await TemplateTextboxUpdateNew(type);
        await UpdateSampleBox(_currentTitlePackage, _templateFields[_currentTitlePackage].Value);
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


    private async Task SaveConfiguration()
    {
        var isValid = await ValidateConfiguration();
        if (!isValid)
            return;
        await SettingsService.SavePackageRenamerSettings(_settings);
        var notificationMessage = new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Configuration Saved",
            Duration = 4000
        };
        NotificationService.Notify(notificationMessage);
        await ResetGrid();
    }

    #region UI Helpers

    private async Task<bool> LoadDefaultTemplate()
    {
        var confirmationResult = await DialogService.Confirm(
            $"This action will replace current template do you want to continue?", "Default Template",
            new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" });

        if (confirmationResult is not true) return true;
        const string nspBaseTemplate = @"{BasePath}\NSP\Base\{TitleName} [{TitleId}][v{Version}].{Extension}";
        const string nspUpdateTemplate = @"{BasePath}\NSP\Updates\{AppName} [{TitleId}][v{Version}].{Extension}";
        const string nspDlcTemplate = @"{BasePath}\NSP\DLC\{AppName} [{TitleName}][{TitleId}][v{Version}].{Extension}";
        
        const string xciBaseTemplate = @"{BasePath}\XCI\Base\{TitleName} [{TitleId}][v{Version}].{Extension}";
        const string xciUpdateTemplate = @"{BasePath}\XCI\Updates\{AppName} [{TitleId}][v{Version}].{Extension}";
        const string xciDlcTemplate = @"{BasePath}\XCI\DLC\{AppName} [{TitleName}][{TitleId}][v{Version}].{Extension}";
        
        _templateFields[TitlePackageType.NspBase].Value = nspBaseTemplate.Replace(@"\", Path.DirectorySeparatorChar.ToString());
        _templateFields[TitlePackageType.NspUpdate].Value = nspUpdateTemplate.Replace(@"\", Path.DirectorySeparatorChar.ToString());
        _templateFields[TitlePackageType.NspDlc].Value = nspDlcTemplate.Replace(@"\", Path.DirectorySeparatorChar.ToString());
        _templateFields[TitlePackageType.NszBase].Value = nspBaseTemplate.Replace(@"\", Path.DirectorySeparatorChar.ToString());
        
        _templateFields[TitlePackageType.NszUpdate].Value = nspUpdateTemplate.Replace(@"\", Path.DirectorySeparatorChar.ToString());
        _templateFields[TitlePackageType.NszDlc].Value = nspDlcTemplate.Replace(@"\", Path.DirectorySeparatorChar.ToString());
        
        _templateFields[TitlePackageType.XciBase].Value = xciBaseTemplate.Replace(@"\", Path.DirectorySeparatorChar.ToString());
        _templateFields[TitlePackageType.XciUpdate].Value = xciUpdateTemplate.Replace(@"\", Path.DirectorySeparatorChar.ToString());
        _templateFields[TitlePackageType.XciDlc].Value = xciDlcTemplate.Replace(@"\", Path.DirectorySeparatorChar.ToString());        
        
        _templateFields[TitlePackageType.XczBase].Value = xciBaseTemplate.Replace(@"\", Path.DirectorySeparatorChar.ToString());
        _templateFields[TitlePackageType.XczUpdate].Value = xciUpdateTemplate.Replace(@"\", Path.DirectorySeparatorChar.ToString());
        _templateFields[TitlePackageType.XczDlc].Value = xciDlcTemplate.Replace(@"\", Path.DirectorySeparatorChar.ToString());        
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

        _settings.NspBasePath = _templateFields[TitlePackageType.NspBase].Value;
        _settings.NspDlcPath = _templateFields[TitlePackageType.NspDlc].Value;
        _settings.NspUpdatePath = _templateFields[TitlePackageType.NspUpdate].Value;
        _settings.NszBasePath = _templateFields[TitlePackageType.NszBase].Value;
        _settings.NszDlcPath = _templateFields[TitlePackageType.NszDlc].Value;
        _settings.NszUpdatePath = _templateFields[TitlePackageType.NszUpdate].Value;
        _settings.XciBasePath = _templateFields[TitlePackageType.XciBase].Value;
        _settings.XciDlcPath = _templateFields[TitlePackageType.XciDlc].Value;
        _settings.XciUpdatePath = _templateFields[TitlePackageType.XciUpdate].Value;
        _settings.XczBasePath = _templateFields[TitlePackageType.XczBase].Value;
        _settings.XczDlcPath = _templateFields[TitlePackageType.XczDlc].Value;
        _settings.XczUpdatePath = _templateFields[TitlePackageType.XczUpdate].Value;

        NotificationService.Notify(notificationMessage);
        return validationResult.IsValid;
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
                "The first name among the list of declared titles, or the one coming from titledb",
            TemplateField.TitleId   => "The title id eg [0100F2200C984000]",
            TemplateField.Version   => "The version of the title eg 65536",
            TemplateField.Extension => "The extension of the file based on its contents eg .nsp",
            TemplateField.AppName =>
                "The name the corresponding Application, useful in updates and dlc to see the Application they belong to",
            TemplateField.PatchId =>
                "If content is an Application, this value is equal to the id of the corresponding Patch content, otherwise empty",
            TemplateField.PatchCount =>
                "If content is an Application, this value is equal to the number of patches available for the corresponding Application, otherwise empty",
            _ => string.Empty
        };

        TooltipService.Open(elementReference, content, options);
    }

    #endregion
}

public record TemplateFieldInfo
{
    public required TitlePackageType FieldType { get; init; }
    public int CursorPosition { get; set; }
    public required string Value { get; set; }
}