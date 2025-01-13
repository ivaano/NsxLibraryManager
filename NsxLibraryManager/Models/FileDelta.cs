namespace NsxLibraryManager.Models;

public class FileDelta
{
    public required IEnumerable<string> FilesToAdd { get; init; }
    public required IEnumerable<string> FilesToRemove { get; init; }
    public required IEnumerable<string> FilesToUpdate { get; init; }

}