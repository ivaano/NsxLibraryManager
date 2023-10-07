namespace NsxLibraryManager.Services;

public interface ITitleLibraryService
{
    public bool DropLibraryAsync();
    public Task<bool> ProcessFileAsync(string file);

    public Task<IEnumerable<string>> GetFilesAsync();
}