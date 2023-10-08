namespace NsxLibraryManager.Services;

public interface ITitleDbService
{
    public Task ImportRegionAsync(string region);
    
    public Task ImportCnmtsAsync();
    
    public IEnumerable<string> GetRegionsToImport();

}