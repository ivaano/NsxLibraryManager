using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;

namespace NsxLibraryManager.Pages;

public partial class Renamer
{
    [Inject] 
    TooltipService tooltipService { get; set; } = default!;

    private readonly Variant variant = Variant.Outlined;
    private string inputPath;
    private string nspBase;
    private string nspDlc;
    private string nspUpdate;
    private string nszBase;
    private string nszDlc;
    private string nszUpdate;
    private string xciBase;
    private string xciDlc;
    private string xciUpdate;
    private string xczBase;
    private string xczDlc;
    private string xczUpdate;
    private bool recursive;
    private string outputBasePath;
    private TextBoxType currentTextBox = TextBoxType.None;
    private const string BasePathField = "{BasePath}";
    private const string TitleNameField = "{TitleName}";
    private const string TitleIdField = "{TitleId}";
    private const string VersionField = "{Version}";
    private const string ExtensionField = "{Extension}";
    private const string AppNameField = "{AppName}";
    private const string PatchIdField = "{PatchId}";
    private const string PatchNumField = "{PatchNum}";
    

    private void ShowTooltip(TemplateField templateField, ElementReference elementReference)
    {
        string? content;
        var options = new TooltipOptions()
        {
                Delay = 200,
                Position = TooltipPosition.Top,
        };
        
        if (templateField is TemplateField.Extension or TemplateField.AppName or TemplateField.PatchId or TemplateField.PatchNum)
            options.Position = TooltipPosition.Bottom;

        content = templateField switch
        {
                TemplateField.BasePath => "Path from the output field Base Path",
                TemplateField.TitleName =>
                        "The first name among the list of declared titles, or the one coming from titledb",
                TemplateField.TitleId => "The title id eg [0100F2200C984000]",
                TemplateField.Version => "The version of the title eg 65536",
                TemplateField.Extension => "The extension of the file based on its contents eg .nsp",
                TemplateField.AppName => "The name the corresponding Application, useful in updates and dlc to see the Application they belong to",
                TemplateField.PatchId => "If content is an Application, this value is equal to the id of the corresponding Patch content, otherwise empty",
                TemplateField.PatchNum => "If content is an Application, this value is equal to the number of patches available for the corresponding Application, otherwise empty",
                _ => string.Empty
        };

        tooltipService.Open(elementReference, content, options);
    }


    private async Task TemplateFieldClick(TemplateField type, MouseEventArgs args)
    {
        var templateField = type switch
        {
                TemplateField.BasePath => BasePathField,
                TemplateField.TitleName => TitleNameField,
                TemplateField.TitleId => TitleIdField,
                TemplateField.Version => VersionField,
                TemplateField.Extension => ExtensionField,
                TemplateField.AppName => AppNameField,
                TemplateField.PatchId => PatchIdField,
                TemplateField.PatchNum => PatchNumField,
                _ => string.Empty
        };

        switch (currentTextBox)
        {
            case TextBoxType.NspBase:
                nspBase += templateField;
                break;
            case TextBoxType.NspDlc:
                nspDlc += templateField;
                break;
            case TextBoxType.NspUpdate:
                nspUpdate += templateField;
                break;
            case TextBoxType.NszBase:
                nszBase += templateField;
                break;
            case TextBoxType.NszDlc:
                nszDlc += templateField;
                break;
            case TextBoxType.NszUpdate:
                nszUpdate += templateField;
                break;
            case TextBoxType.XciBase:
                xciBase += templateField;
                break;
            case TextBoxType.XciDlc:
                xciDlc += templateField;
                break;
            case TextBoxType.XciUpdate:
                xciUpdate += templateField;
                break;
            case TextBoxType.XczBase:
                xczBase += templateField;
                break;
            case TextBoxType.XczDlc:
                xczDlc += templateField;
                break;
            case TextBoxType.XczUpdate:
                xczUpdate += templateField;
                break;
            case TextBoxType.None:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task TemplateTextboxChange(TextBoxType type, MouseEventArgs args)
    {
        currentTextBox = type;
    }

 
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
