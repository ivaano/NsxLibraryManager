using Newtonsoft.Json.Linq;
using NsxLibraryManager.Models;
using NsxLibraryManager.Repository;
using Radzen;

namespace NsxLibraryManager.Services;

public interface IDataService : IDisposable
{
    new void Dispose();
  
    public IRegionRepository RegionRepository(string region);
    public int ImportTitleDbRegionTitles(JObject titles, string region);
    public int ImportTitleDbCnmts(List<PackagedContentMeta> packagedContentMeta);
    public int ImportTitleDbVersions(List<GameVersions> gameVersions);
    public IEnumerable<GameVersions> GetTitleDbVersions(string titleTitleId);
    public Task<IEnumerable<RegionTitle>?> GetTitleDbRegionTitlesAsync(string region);
    public Task<RegionTitle?> GetTitleDbRegionTitleByIdAsync(string region, string titleId);
    public IEnumerable<PackagedContentMeta> GetTitleDbCnmtsForTitle(string titleId);
    public Task<IEnumerable<LibraryTitle>> GetLibraryTitlesAsync();
    public IQueryable<LibraryTitle> GetLibraryTitlesQueryableAsync();
    public Task<IQueryable<RegionTitle>> GetTitleDbRegionTitlesQueryableAsync(string region);
    public LibraryTitle? GetLibraryTitleById(string titleId);
    public void UpdateLibraryTitleAsync(LibraryTitle libraryTitle);
    public Task AddLibraryTitleAsync(LibraryTitle libraryTitle);
    public bool DropDbCollection(string collectionName);
    public DateTime? GetRegionLastUpdate(string region, CancellationToken cancellationToken = default);
    
    public Task SaveDataGridStateAsync(string name, DataGridSettings settings);

    public Task<DataGridSettings?> LoadDataGridStateAsync(string name);
}