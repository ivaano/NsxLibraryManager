namespace NsxLibraryManager.Shared.Dto;

public class LibraryLocationDto
{
    public string Path { get; set; }
    public bool Recursive { get; set; }
    public int CollectionId { get; set; }
}