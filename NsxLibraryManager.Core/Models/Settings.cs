namespace NsxLibraryManager.Core.Models;

public record Settings
{
    public int Id { get; set; }
    public required string Name { get; init; }
    public required string Value { get; set; }
}