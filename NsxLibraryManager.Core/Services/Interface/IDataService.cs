using Newtonsoft.Json.Linq;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Models.Stats;
using NsxLibraryManager.Core.Repository.Interface;

namespace NsxLibraryManager.Core.Services.Interface;

public interface IDataService : IDisposable
{
    new void Dispose();
  
    public IRegionRepository RegionRepository(string region);
    public int ImportTitleDbRegionTitles(JObject titles, string region);
    public int ImportTitleDbCnmts(List<PackagedContentMeta> packagedContentMeta);
    public int ImportTitleDbVersions(List<GameVersions> gameVersions);
    public IEnumerable<GameVersions> GetTitleDbVersions(string titleTitleId);
    public Task<IQueryable<RegionTitle>> GetTitleDbRegionTitlesQueryableAsync(string region);
    public Task<IEnumerable<RegionTitle>?> GetTitleDbRegionTitlesAsync(string region);
    public Task<RegionTitle?> GetTitleDbRegionTitleByIdAsync(string region, string titleId);
    public IEnumerable<PackagedContentMeta> GetTitleDbCnmtsForTitle(string titleId);
    public Task<IEnumerable<LibraryTitle>> GetLibraryTitlesAsync();
    public IQueryable<LibraryTitle> GetLibraryTitlesQueryableAsync();
    public LibraryTitle? GetLibraryTitleById(string titleId);
    public void UpdateLibraryTitleAsync(LibraryTitle libraryTitle);
    public bool DeleteLibraryTitle(string titleId);
    public Task AddLibraryTitleAsync(LibraryTitle? libraryTitle);
    public bool DropDbCollection(string collectionName);
    public DateTime? GetRegionLastUpdate(string region, CancellationToken cancellationToken = default);
    public ContentDistribution GetContentDistribution();
    public PackageDistribution GetPackageDistribution();
    public LibraryStats GetLibraryTitlesStats();
    //public Task SaveDataGridStateAsync(string name, DataGridSettings settings);

    //public Task<DataGridSettings?> LoadDataGridStateAsync(string name);
}

