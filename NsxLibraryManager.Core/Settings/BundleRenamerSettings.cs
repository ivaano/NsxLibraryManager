namespace NsxLibraryManager.Core.Settings;

public class BundleRenamerSettings
{
    public string InputPath { get; set; } = string.Empty;
    public bool Recursive { get; set; } = true;
    public string OutputBasePath { get; set; } = string.Empty;
    
    public string BundleBase { get; set; } = string.Empty;
    public string BundleDlc { get; set; } = string.Empty;
    public string BundleUpdate { get; set; } = string.Empty;
    
    public string UnknownPlaceholder { get; set; } = "Unknown";
    public bool TitlesForceUppercase { get; set ; } = true;
}