using System.ComponentModel.DataAnnotations;

namespace NsxLibraryManager.Core.Settings;

public class RenamerSettings
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
}