namespace NsxLibraryManager.Services;

public interface ITitleDbService
{
    public Task ImportRegionAsync(string region);
    
    public IEnumerable<string> GetRegionsToImport();

}