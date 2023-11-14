using NsxLibraryManager.Models;

namespace NsxLibraryManager.Services.Interface;

public interface ITitleLibraryService
{
    public bool DropLibrary();
    public string GetLibraryPath();
    public LibraryTitle? GetTitle(string titleId);
    public Task<LibraryTitle?> GetTitleFromTitleDb(string titleId);
    public Task<bool> ProcessFileAsync(string file);
    public Task AddOwnedDlcToTitlesAsync();
    public Task AddOwnedUpdateToTitlesAsync();
    public Task<IEnumerable<string>> GetFilesAsync();
    

}