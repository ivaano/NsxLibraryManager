namespace NsxLibraryManager.Services;

public interface ITitleDbService
{
    public Task ImportRegionAsync(string region);
    
    public Task ImportCnmtsAsync();
    
    public Task ImportVersionsAsync();
    
    public IEnumerable<string> GetRegionsToImport();

}