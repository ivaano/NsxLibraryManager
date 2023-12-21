using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;

namespace NsxLibraryManager.Pages;

public partial class Renamer
{
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    [Inject] private TooltipService TooltipService { get; set; } = default!;

    private readonly Variant _variant = Variant.Outlined;
    private string _inputPath = string.Empty;
    private string _nspBase = string.Empty;
    private string _nspDlc = string.Empty;
    private string _nspUpdate = string.Empty;
    private string _nszBase = string.Empty;
    private string _nszDlc = string.Empty;
    private string _nszUpdate = string.Empty;
    private string _xciBase  = string.Empty;
    private string _xciDlc = string.Empty;
    private string _xciUpdate = string.Empty;
    private string _xczBase = string.Empty;
    private string _xczDlc = string.Empty;
    private string _xczUpdate = string.Empty;
    private string _sampleResultLabel = "Sample Result";
    private readonly Dictionary<TextBoxType, TemplateFieldInfo> _templateFields = new();
    private Dictionary<TextBoxType, Action<string>> _textBoxTypeActions = new();

    private readonly Dictionary<TemplateField, string> _templateFieldMappings = new()
    {
        { TemplateField.BasePath, "{BasePath}" },
        { TemplateField.TitleName, "{TitleName}" },
        { TemplateField.TitleId, "{TitleId}" },
        { TemplateField.Version, "{Version}" },
        { TemplateField.Extension, "{Extension}" },
        { TemplateField.AppName, "{AppName}" },
        { TemplateField.PatchId, "{PatchId}" },
        { TemplateField.PatchNum, "{PatchNum}" },
    };

    private bool _recursive = true;
    private string _outputBasePath = string.Empty;
    private TextBoxType _currentTextBox = TextBoxType.None;


    protected override async Task OnInitializedAsync()
    {
        InitializeTemplateFields();
        InitializeTextBoxTypeActions();
        await base.OnInitializedAsync();
    }

    private void InitializeTemplateFields()
    {
        var textBoxTypes = new[]
        {
            TextBoxType.NspBase,
            TextBoxType.NspDlc,
            TextBoxType.NspUpdate,
            TextBoxType.NszBase,
            TextBoxType.NszDlc,
            TextBoxType.NszUpdate,
            TextBoxType.XciBase,
            TextBoxType.XciDlc,
            TextBoxType.XciUpdate,
            TextBoxType.XczBase,
            TextBoxType.XczDlc,
            TextBoxType.XczUpdate
        };

        foreach (var textBoxType in textBoxTypes)
        {
            _templateFields[textBoxType] = CreateTemplateFieldInfo(textBoxType);
        }
    }

    private void InitializeTextBoxTypeActions()
    {
        _textBoxTypeActions = new Dictionary<TextBoxType, Action<string>>
        {
            { TextBoxType.NspBase, (templateField) => UpdateTemplateFieldRecord(templateField, ref _nspBase) },
            { TextBoxType.NspDlc, (templateField) => UpdateTemplateFieldRecord(templateField, ref _nspDlc) },
            { TextBoxType.NspUpdate, (templateField) => UpdateTemplateFieldRecord(templateField, ref _nspUpdate) },
            { TextBoxType.NszBase, (templateField) => UpdateTemplateFieldRecord(templateField, ref _nszBase) },
            { TextBoxType.NszDlc, (templateField) => UpdateTemplateFieldRecord(templateField, ref _nszDlc) },
            { TextBoxType.NszUpdate, (templateField) => UpdateTemplateFieldRecord(templateField, ref _nszUpdate) },
            { TextBoxType.XciBase, (templateField) => UpdateTemplateFieldRecord(templateField, ref _xciBase) },
            { TextBoxType.XciDlc, (templateField) => UpdateTemplateFieldRecord(templateField, ref _xciDlc) },
            { TextBoxType.XciUpdate, (templateField) => UpdateTemplateFieldRecord(templateField, ref _xciUpdate) },
            { TextBoxType.XczBase, (templateField) => UpdateTemplateFieldRecord(templateField, ref _xczBase) },
            { TextBoxType.XczDlc, (templateField) => UpdateTemplateFieldRecord(templateField, ref _xczDlc) },
            { TextBoxType.XczUpdate, (templateField) => UpdateTemplateFieldRecord(templateField, ref _xczUpdate) },
        };
    }

    private static TemplateFieldInfo CreateTemplateFieldInfo(TextBoxType fieldType)
    {
        return new TemplateFieldInfo
        {
            FieldType = fieldType,
            Value = string.Empty
        };
    }

    private void ShowTooltip(TemplateField templateField, ElementReference elementReference)
    {
        var options = new TooltipOptions()
        {
                Delay = 200,
                Position = TooltipPosition.Top,
        };

        if (templateField is TemplateField.Extension or TemplateField.AppName or TemplateField.PatchId
            or TemplateField.PatchNum)
            options.Position = TooltipPosition.Bottom;

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
        var pos = _templateFields[_currentTextBox].CursorPosition;
        var fieldContent = pos > 0 ? currentValue.Insert(pos, templateField) : templateField;
        _templateFields[_currentTextBox].CursorPosition += templateField.Length;
        _templateFields[_currentTextBox].Value = fieldContent;
        currentValue = fieldContent;
    }


    private async Task TemplateFieldClick(TemplateField templateFieldType, MouseEventArgs args)
    {
        if (_templateFieldMappings.TryGetValue(templateFieldType, out var templateField) &&
            _textBoxTypeActions.TryGetValue(_currentTextBox, out var action))
        {
            action(templateField);
        }


        if (_currentTextBox != TextBoxType.None)
            await JsRuntime.InvokeVoidAsync("setFocus", _currentTextBox.ToString(), " {0}");
    }


    private async Task TemplateTextboxUpdate(TextBoxType type)
    {
        _currentTextBox = type;
        _sampleResultLabel = _currentTextBox.ToString();
        if (type == TextBoxType.None)
        {
            _sampleResultLabel = "Sample Result";
            return;
        }
        _templateFields[type].CursorPosition =
                await JsRuntime.InvokeAsync<int>("getCursorLocation", type.ToString(), " {0}");

    }
}

public record TemplateFieldInfo
{
    public required TextBoxType FieldType { get; init; }
    public int CursorPosition { get; set; }
    public required string Value { get; set; }
}

public enum TextBoxType
{
    None,
    NspBase,
    NspDlc,
    NspUpdate,
    NszBase,
    NszDlc,
    NszUpdate,
    XciBase,
    XciDlc,
    XciUpdate,
    XczBase,
    XczDlc,
    XczUpdate
}

public enum TemplateField
{
    BasePath,
    TitleName,
    TitleId,
    Version,
    Extension,
    AppName,
    PatchId,
    PatchNum
}