namespace NsxLibraryManager.Services;

public interface ITitleLibraryService
{
    //drops collection and scan library folder
    public Task RefreshLibraryAsync();
    
    public Task<bool> ProcessFileAsync(string file);

    public Task<IEnumerable<string>> GetFilesAsync();
}