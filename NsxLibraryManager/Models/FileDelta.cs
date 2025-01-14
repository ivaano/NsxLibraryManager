using NsxLibraryManager.Core.Models;

namespace NsxLibraryManager.Models;

public class FileDelta
{
    public required IEnumerable<LibraryTitle> FilesToAdd { get; init; }
    public required IEnumerable<LibraryTitle> FilesToRemove { get; init; }
    public required IEnumerable<LibraryTitle> FilesToUpdate { get; init; }
    
    public int TotalFiles { get; init; }

}