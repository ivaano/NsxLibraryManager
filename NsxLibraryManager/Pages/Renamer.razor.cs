using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Mapping;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Core.Settings;
using Radzen;

namespace NsxLibraryManager.Pages;

public partial class Renamer
{
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    [Inject] private TooltipService TooltipService { get; set; } = default!;

    [Inject] private IRenamerService RenamerService { get; set; } = default!;

    [Inject] private NotificationService NotificationService { get; set; } = default!;

    private const Variant Variant = Radzen.Variant.Outlined;
    private string _nspBase = string.Empty;
    private string _nspDlc = string.Empty;
    private string _nspUpdate = string.Empty;
    private string _nszBase = string.Empty;
    private string _nszDlc = string.Empty;
    private string _nszUpdate = string.Empty;
    private string _xciBase = string.Empty;
    private string _xciDlc = string.Empty;
    private string _xciUpdate = string.Empty;
    private string _xczBase = string.Empty;
    private string _xczDlc = string.Empty;
    private string _xczUpdate = string.Empty;
    private string _sampleBefore = string.Empty;
    private string _sampleAfter = string.Empty;
    private string _sampleResultLabel = "Sample Result";
    private PackageTitleType _currentPackageTitle = PackageTitleType.None;
    private readonly Dictionary<PackageTitleType, TemplateFieldInfo> _templateFields = new();

    private readonly Dictionary<TemplateField, string> _templateFieldMappings =
        RenamerTemplateFields.TemplateFieldMappings;

    private Dictionary<PackageTitleType, Action<string>> _textBoxTypeActions = new();

    private readonly Dictionary<string, string> _validationErrors = new()
    {
        { "InputPath", string.Empty },
        { "OutputBasePath", string.Empty }
    };

    private RenamerSettings _settings = default!;

    protected override async Task OnInitializedAsync()
    {
        InitializeTemplateFields();
        InitializeTextBoxTypeActions();
        await InitializeSettings();
        await base.OnInitializedAsync();
    }

    private async Task InitializeSettings()
    {
        _settings = await RenamerService.LoadRenamerSettingsAsync();
        _nspBase = _settings.NspBasePath;
        _nspDlc = _settings.NspDlcPath;
        _nspUpdate = _settings.NspUpdatePath;
    }

    private void InitializeTemplateFields()
    {
        var textBoxTypes = new[]
        {
            PackageTitleType.NspBase,
            PackageTitleType.NspDlc,
            PackageTitleType.NspUpdate,
            PackageTitleType.NszBase,
            PackageTitleType.NszDlc,
            PackageTitleType.NszUpdate,
            PackageTitleType.XciBase,
            PackageTitleType.XciDlc,
            PackageTitleType.XciUpdate,
            PackageTitleType.XczBase,
            PackageTitleType.XczDlc,
            PackageTitleType.XczUpdate
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

    private void InitializeTextBoxTypeActions()
    {
        _textBoxTypeActions = new Dictionary<PackageTitleType, Action<string>>
        {
            { PackageTitleType.NspBase, (templateField) => UpdateTemplateFieldRecord(templateField, ref _nspBase) },
            { PackageTitleType.NspDlc, (templateField) => UpdateTemplateFieldRecord(templateField, ref _nspDlc) },
            { PackageTitleType.NspUpdate, (templateField) => UpdateTemplateFieldRecord(templateField, ref _nspUpdate) },
            { PackageTitleType.NszBase, (templateField) => UpdateTemplateFieldRecord(templateField, ref _nszBase) },
            { PackageTitleType.NszDlc, (templateField) => UpdateTemplateFieldRecord(templateField, ref _nszDlc) },
            { PackageTitleType.NszUpdate, (templateField) => UpdateTemplateFieldRecord(templateField, ref _nszUpdate) },
            { PackageTitleType.XciBase, (templateField) => UpdateTemplateFieldRecord(templateField, ref _xciBase) },
            { PackageTitleType.XciDlc, (templateField) => UpdateTemplateFieldRecord(templateField, ref _xciDlc) },
            { PackageTitleType.XciUpdate, (templateField) => UpdateTemplateFieldRecord(templateField, ref _xciUpdate) },
            { PackageTitleType.XczBase, (templateField) => UpdateTemplateFieldRecord(templateField, ref _xczBase) },
            { PackageTitleType.XczDlc, (templateField) => UpdateTemplateFieldRecord(templateField, ref _xczDlc) },
            { PackageTitleType.XczUpdate, (templateField) => UpdateTemplateFieldRecord(templateField, ref _xczUpdate) },
        };
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
            or TemplateField.PatchNum)
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
            TemplateField.PatchNum =>
                "If content is an Application, this value is equal to the number of patches available for the corresponding Application, otherwise empty",
            _ => string.Empty
        };

        TooltipService.Open(elementReference, content, options);
    }

    private void UpdateTemplateFieldRecord(string templateField, ref string currentValue)
    {
        var pos = _templateFields[_currentPackageTitle].CursorPosition;
        var fieldContent = pos > 0 ? currentValue.Insert(pos, templateField) : templateField;
        _templateFields[_currentPackageTitle].CursorPosition += templateField.Length;
        _templateFields[_currentPackageTitle].Value = fieldContent;
        currentValue = fieldContent;
    }


    private async Task TemplateFieldClick(TemplateField templateFieldType, MouseEventArgs args)
    {
        if (_templateFieldMappings.TryGetValue(templateFieldType, out var templateField) &&
            _textBoxTypeActions.TryGetValue(_currentPackageTitle, out var action))
        {
            action(templateField);
        }

        if (_currentPackageTitle != PackageTitleType.None)
            await JsRuntime.InvokeVoidAsync("setFocus", _currentPackageTitle.ToString(), " {0}");
    }

    private async Task UpdateSampleBox(PackageTitleType type)
    {
        _sampleBefore = $"c:\\dump\\lucas-game.nsp";
        _sampleAfter = await RenamerService.CalculateSampleFileName(_templateFields[_currentPackageTitle].Value, type, "inputFile.nsp", "basePath");
    }

    private async Task TemplateTextboxUpdate(PackageTitleType type)
    {
        _currentPackageTitle = type;
        var labelParts = Regex.Split(_currentPackageTitle.ToString(), "(?<!^)(?=[A-Z])");
        _sampleResultLabel = string.Join(" ", labelParts);
        if (type == PackageTitleType.None)
        {
            _sampleResultLabel = "Sample Result";
            return;
        }

        //await UpdateSampleBox(type);
        _templateFields[type].CursorPosition =
            await JsRuntime.InvokeAsync<int>("getCursorLocation", type.ToString(), " {0}");
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

        _settings.NspBasePath = _nspBase;
        _settings.NspDlcPath = _nspDlc;
        _settings.NspUpdatePath = _nspUpdate;
        NotificationService.Notify(notificationMessage);
        return validationResult.IsValid;
    }

    private async Task SaveConfiguration()
    {
        var isValid = await ValidateConfiguration();
        if (!isValid)
            return;
        var savedSettings = await RenamerService.SaveRenamerSettingsAsync(_settings);

        var notificationMessage = new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Configuration Saved",
            Duration = 4000
        };
        NotificationService.Notify(notificationMessage);
    }
}

public record TemplateFieldInfo
{
    public required PackageTitleType FieldType { get; init; }
    public int CursorPosition { get; set; }
    public required string Value { get; set; }
}