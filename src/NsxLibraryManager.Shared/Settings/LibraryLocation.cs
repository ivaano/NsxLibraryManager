namespace NsxLibraryManager.Shared.Settings;

public class LibraryLocation
{
    public string Path { get; set; } = string.Empty;
    public bool Recursive { get; set; }
    public int CollectionId { get; set; }
}