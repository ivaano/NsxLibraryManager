namespace NsxLibraryManager.Services;

public interface ITitleLibraryService
{
    public bool DropLibrary();
    public string GetLibraryPath();
    public Task<bool> ProcessFileAsync(string file);
    public Task AddOwnedDlcToTitlesAsync();
    public Task<IEnumerable<string>> GetFilesAsync();
}