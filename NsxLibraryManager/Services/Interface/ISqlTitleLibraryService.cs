using NsxLibraryManager.Core.Models;

namespace NsxLibraryManager.Services.Interface;

public interface ISqlTitleLibraryService
{
    public Task<bool> DropLibrary();
    public Task<IEnumerable<string>> GetFilesAsync();
    
    public Task<LibraryTitle?> ProcessFileAsync(string file);
    public Task<bool> SaveDatabaseChangesAsync();

}