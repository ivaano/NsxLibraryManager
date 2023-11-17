namespace NsxLibraryManager.Models;

public class PackagedContentMeta
{
    public List<ContentEntry>? ContentEntries { get; set; }
    public List<MetaEntry> MetaEntries { get; set; }
    public string OtherApplicationId { get; set; }
    public int? RequiredApplicationVersion { get; set; }
    public int? RequiredSystemVersion { get; set; }
    public string TitleId { get; set; }
    public int TitleType { get; set; }
    public string Version { get; set; }
}

public class ContentEntry
{
    public string BuildId { get; set; }
    public string NcaId { get; set; }
    public int Type { get; set; }
}

public class MetaEntry
{
    // No properties here
}

