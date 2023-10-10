using Newtonsoft.Json.Linq;
using NsxLibraryManager.Models;
using NsxLibraryManager.Repository;

namespace NsxLibraryManager.Services;

public interface IDataService : IDisposable
{
    new void Dispose();
  
    public IRegionRepository RegionRepository(string region);
    public int ImportTitleDbRegionTitles(JObject titles, string region);
    public int ImportTitleDbCnmts(List<PackagedContentMeta> packagedContentMeta);
    public int ImportTitleDbVersions(List<GameVersions> gameVersions);
    public Task<IEnumerable<RegionTitle>> GetRegionTitlesAsync(string region);
    public Task<IEnumerable<LibraryTitle>> GetLibraryTitlesAsync();
    public Task AddLibraryTitleAsync(LibraryTitle libraryTitle);
    public bool DropDbCollection(string collectionName);
    public DateTime? GetRegionLastUpdate(string region, CancellationToken cancellationToken = default);
}