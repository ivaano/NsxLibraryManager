using NsxLibraryManager.Core.Enums;

namespace NsxLibraryManager.Core.Mapping;

public class RenamerTemplateFields
{

    public static readonly Dictionary<TemplateField, string> TemplateFieldMappings = new()
    {
        { TemplateField.BasePath, "{BasePath}" },
        { TemplateField.TitleName, "{TitleName}" },
        { TemplateField.TitleId, "{TitleId}" },
        { TemplateField.Version, "{Version}" },
        { TemplateField.Extension, "{Extension}" },
        { TemplateField.AppName, "{AppName}" },
        { TemplateField.PatchId, "{PatchId}" },
        { TemplateField.PatchCount, "{PatchCount}" },
        { TemplateField.DlcCount, "{DlcCount}" },
        { TemplateField.Region, "{Region}" },
        { TemplateField.Size, "{Size}" },
        { TemplateField.CollectionName, "{Collection}" },
    };
    
    public static string GetTemplateField(TemplateField templateField)
    {
        return TemplateFieldMappings.TryGetValue(templateField, out var value) ? value : string.Empty;
    }
    
}