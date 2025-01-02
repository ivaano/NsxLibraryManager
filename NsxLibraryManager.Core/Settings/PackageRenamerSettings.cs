using System.ComponentModel.DataAnnotations;

namespace NsxLibraryManager.Core.Settings;

public class PackageRenamerSettings
{
    public string InputPath { get; set; } = string.Empty;
    public bool Recursive { get; set; } = true;
    public string OutputBasePath { get; set; } = string.Empty;
    public string NspBasePath { get; set; } = string.Empty;
    public string NspDlcPath { get; set; } = string.Empty;
    public string NspUpdatePath { get; set; } = string.Empty;
    public string NszBasePath { get; set; } = string.Empty;
    public string NszDlcPath { get; set; } = string.Empty;
    public string NszUpdatePath { get; set; } = string.Empty;
    public string XciBasePath { get; set; } = string.Empty;
    public string XciDlcPath { get; set; } = string.Empty;
    public string XciUpdatePath { get; set; } = string.Empty;
    public string XczBasePath { get; set; } = string.Empty;
    public string XczDlcPath { get; set; } = string.Empty;
    public string XczUpdatePath { get; set; } = string.Empty;
    
    public string UnknownPlaceholder { get; set; } = "UnknownTitle";

}