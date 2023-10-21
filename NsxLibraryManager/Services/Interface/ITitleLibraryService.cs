using NsxLibraryManager.Models;

namespace NsxLibraryManager.Services;

public interface ITitleLibraryService
{
    public bool DropLibrary();
    public string GetLibraryPath();
    public LibraryTitle? GetTitle(string titleId);
    public Task<LibraryTitle?> GetTitleFromTitleDb(string titleId);
    public Task<bool> ProcessFileAsync(string file);
    public Task AddOwnedDlcToTitlesAsync();
    public Task<IEnumerable<string>> GetFilesAsync();
    

}