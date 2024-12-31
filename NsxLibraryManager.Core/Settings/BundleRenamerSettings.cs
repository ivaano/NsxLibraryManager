namespace NsxLibraryManager.Core.Settings;

public class BundleRenamerSettings
{
    public string InputPath { get; set; } = string.Empty;
    public bool Recursive { get; set; } = true;
    public string OutputBasePath { get; set; } = string.Empty;
    
    public string BasePath { get; set; } = string.Empty;
    public string DlcPath { get; set; } = string.Empty;
    public string UpdatePath { get; set; } = string.Empty;
}