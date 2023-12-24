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
    private string _sampleBefore = string.Empty;
    private string _sampleAfter = string.Empty;
    private string _sampleResultLabel = "Sample Result";
    private PackageTitleType _currentPackageTitle = PackageTitleType.None;
    private readonly Dictionary<PackageTitleType, TemplateFieldInfo> _templateFields = new();

    private readonly Dictionary<TemplateField, string> _templateFieldMappings =
        RenamerTemplateFields.TemplateFieldMappings;

    private readonly Dictionary<string, string> _validationErrors = new()
    {
        { "InputPath", string.Empty },
        { "OutputBasePath", string.Empty }
    };

    private bool _fragmentButtonDisabled = true;
    private RenamerSettings _settings = default!;

    protected override async Task OnInitializedAsync()
    {
        InitializeTemplateFields();
        await InitializeSettings();
        await base.OnInitializedAsync();
    }

    private async Task InitializeSettings()
    {
        _settings = await RenamerService.LoadRenamerSettingsAsync();

        _templateFields[PackageTitleType.NspBase].Value = _settings.NspBasePath;
        _templateFields[PackageTitleType.NspDlc].Value = _settings.NspDlcPath;
        _templateFields[PackageTitleType.NspUpdate].Value = _settings.NspUpdatePath;
        _templateFields[PackageTitleType.NszBase].Value = _settings.NszBasePath;
        _templateFields[PackageTitleType.NszDlc].Value = _settings.NszDlcPath;
        _templateFields[PackageTitleType.NszUpdate].Value = _settings.NszUpdatePath;
        _templateFields[PackageTitleType.XciBase].Value = _settings.XciBasePath;
        _templateFields[PackageTitleType.XciDlc].Value = _settings.XciDlcPath;
        _templateFields[PackageTitleType.XciUpdate].Value = _settings.XciUpdatePath;
        _templateFields[PackageTitleType.XczBase].Value = _settings.XczBasePath;
        _templateFields[PackageTitleType.XczDlc].Value = _settings.XczDlcPath;
        _templateFields[PackageTitleType.XczUpdate].Value = _settings.XczUpdatePath;
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

    private async Task UpdateSampleBox(PackageTitleType type, string templateValue)
    {
        _sampleBefore = $"c:\\dump\\lucas-game.nsp";
        _sampleAfter = await RenamerService.CalculateSampleFileName(templateValue, type, "inputFile.nsp", "basePath");
    }

    private async Task TemplateFragmentClick(TemplateField templateFieldType, MouseEventArgs args)
    {
        if (_currentPackageTitle != PackageTitleType.None)
        {
            UpdateTemplateFieldRecord(templateFieldType, _currentPackageTitle);
            await JsRuntime.InvokeVoidAsync("setFocus", _currentPackageTitle.ToString(), " {0}");
        }
    }


    private void UpdateTemplateFieldRecord(TemplateField templateField, PackageTitleType type)
    {
        var templateFieldInfo = _templateFields[_currentPackageTitle];
        var templateFieldValue = _templateFieldMappings[templateField];

        templateFieldInfo.Value = templateFieldInfo.CursorPosition > 0
            ? templateFieldInfo.Value.Insert(templateFieldInfo.CursorPosition, templateFieldValue)
            : templateFieldValue;

        templateFieldInfo.CursorPosition += templateFieldValue.Length;

        Task.Run(() => UpdateSampleBox(_currentPackageTitle, templateFieldInfo.Value));
    }


    private async Task OnTemplateFieldClick(PackageTitleType type, MouseEventArgs args)
    {
        await TemplateTextboxUpdateNew(type);
        await UpdateSampleBox(_currentPackageTitle, _templateFields[_currentPackageTitle].Value);
    }

    private async Task OnTemplateFieldInput(PackageTitleType type, string? value)
    {
        if (value is not null)
        {
            _templateFields[type].Value = value;
            await TemplateTextboxUpdateNew(type);
            await UpdateSampleBox(_currentPackageTitle, value);
        }
    }

    private async Task TemplateTextboxUpdateNew(PackageTitleType type)
    {
        _currentPackageTitle = type;
        var labelParts = Regex.Split(_currentPackageTitle.ToString(), "(?<!^)(?=[A-Z])");
        _sampleResultLabel = string.Join(" ", labelParts);
        if (type == PackageTitleType.None)
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
        var savedSettings = await RenamerService.SaveRenamerSettingsAsync(_settings);

        var notificationMessage = new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Configuration Saved",
            Duration = 4000
        };
        NotificationService.Notify(notificationMessage);
    }

    #region UI Helpers

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

        _settings.NspBasePath = _templateFields[PackageTitleType.NspBase].Value;
        _settings.NspDlcPath = _templateFields[PackageTitleType.NspDlc].Value;
        _settings.NspUpdatePath = _templateFields[PackageTitleType.NspUpdate].Value;
        _settings.NszBasePath = _templateFields[PackageTitleType.NszBase].Value;
        _settings.NszDlcPath = _templateFields[PackageTitleType.NszDlc].Value;
        _settings.NszUpdatePath = _templateFields[PackageTitleType.NszUpdate].Value;
        _settings.XciBasePath = _templateFields[PackageTitleType.XciBase].Value;
        _settings.XciDlcPath = _templateFields[PackageTitleType.XciDlc].Value;
        _settings.XciUpdatePath = _templateFields[PackageTitleType.XciUpdate].Value;
        _settings.XczBasePath = _templateFields[PackageTitleType.XczBase].Value;
        _settings.XczDlcPath = _templateFields[PackageTitleType.XczDlc].Value;
        _settings.XczUpdatePath = _templateFields[PackageTitleType.XczUpdate].Value;

        NotificationService.Notify(notificationMessage);
        return validationResult.IsValid;
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

    #endregion
}

public record TemplateFieldInfo
{
    public required PackageTitleType FieldType { get; init; }
    public int CursorPosition { get; set; }
    public required string Value { get; set; }
}