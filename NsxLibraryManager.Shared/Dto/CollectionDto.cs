namespace NsxLibraryManager.Shared.Dto;

public class CollectionDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int TitlesCount { get; set; }
    public int BaseTitlesCount { get; set; }
    public int DlcTitlesCount { get; set; }
    public int UpdatesTitlesCount { get; set; }
}