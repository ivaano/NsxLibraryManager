namespace NsxLibraryManager.Shared.Settings;

public abstract class RenamerSettings
{
    public string InputPath { get; set; } = string.Empty;
    public bool Recursive { get; set; } = true;
    public string OutputBasePath { get; set; } = string.Empty;
    
    public string UnknownPlaceholder { get; set; } = "Unknown";
    public bool TitlesForceUppercase { get; set ; } = true;
    public bool DeleteEmptyFolders { get; set ; } = true;
    
    public bool UseEnglishNaming { get; set; } = true;

}