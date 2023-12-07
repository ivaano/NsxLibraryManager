using AutoMapper;
using LiteDB;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Repository;
using NsxLibraryManager.Core.Repository.Interface;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Core.Settings;

namespace NsxLibraryManager.Core.Services;

public class DataService : IDataService
{
    private readonly string _connStr;
    private LiteDatabase? _db;
    private bool _disposed;
    private Dictionary<string, IRegionRepository>? _regionRepository;
    private readonly ITitleLibraryRepository _titleLibraryRepository;
    private readonly ITitleDbCnmtsRepository _titleDbCnmtsRepository;
    private ITitleDbVersionsRepository _titleDbVersionsRepository;
    //private ISettingsRepository _settingsRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<DataService> _logger;
    
    private ILiteDatabase Db
    {
        get { return _db ??= new LiteDatabase(_connStr); }
    }
    public IMemoryCache MemoryCache { get; }

    public DataService(IOptions<AppSettings> configuration, IMapper mapper, ILogger<DataService> logger, IMemoryCache memoryCache)
    {
        _logger = logger;
        _mapper = mapper;
        var titleDbLocation = configuration.Value.TitleDatabase ?? throw new InvalidOperationException();
        _connStr = Path.Combine(titleDbLocation);
        _titleDbCnmtsRepository = new TitleDbCnmtsRepository(Db);
        _titleDbVersionsRepository = new TitleDbVersionsRepository(Db);
        _titleLibraryRepository = new TitleLibraryRepository(Db);
        //_settingsRepository = new SettingsRepository(Db);
        MemoryCache = memoryCache;
    }
    
    public IRegionRepository RegionRepository(string region)
    {
        if (_regionRepository is not null)
        {
            if (_regionRepository.TryGetValue(region, out var repository))
                return repository;
        }
        
        _regionRepository = new Dictionary<string, IRegionRepository>()
        {
                [region] = new RegionRepository(Db, region)
        };
        return _regionRepository[region];
    }
    
    public Task<IEnumerable<RegionTitle>?> GetTitleDbRegionTitlesAsync(string region)
    {
        return MemoryCache.GetOrCreateAsync(region, async e =>
        {
            e.SetOptions(new MemoryCacheEntryOptions
            {
                    AbsoluteExpirationRelativeToNow =
                            TimeSpan.FromSeconds(30)
            });

            var regionTitleRepository = RegionRepository(region);
            return await Task.Run(() => regionTitleRepository.All());
        });
    }

    public async Task<RegionTitle?> GetTitleDbRegionTitleByIdAsync(string region, string titleId)
    {
        var regionTitleRepository = RegionRepository(region);
        var regionTitle = regionTitleRepository.FindOne(x => x.TitleId == titleId) ?? regionTitleRepository.FindTitleByIds(titleId);
        return await Task.Run(() => regionTitle);
    }

    public IEnumerable<PackagedContentMeta> GetTitleDbCnmtsForTitle(string titleId)
    {
        return _titleDbCnmtsRepository.FindByOtherApplicationId(titleId);
    }

    public async Task<IEnumerable<LibraryTitle>> GetLibraryTitlesAsync()
    {
        return await Task.Run(() => _titleLibraryRepository.All());
    }

    public IQueryable<LibraryTitle> GetLibraryTitlesQueryableAsync()
    {
        return _titleLibraryRepository.GetTitlesAsQueryable();
    }
    
    public async Task<IQueryable<RegionTitle>> GetTitleDbRegionTitlesQueryableAsync(string region)
    {
        var regionTitleRepository = RegionRepository(region);
        return await Task.Run(() => regionTitleRepository.GetTitlesAsQueryable());
    }
    
    public LibraryTitle? GetLibraryTitleById(string titleId)
    {
        return _titleLibraryRepository.FindOne(x => x.TitleId == titleId);
    }

    public void UpdateLibraryTitleAsync(LibraryTitle libraryTitle)
    {
        _titleLibraryRepository.Update(libraryTitle);
    }
    
    public bool DeleteLibraryTitle(string titleId)
    {
        return _titleLibraryRepository.DeleteTitle(titleId);
    }

    public async Task AddLibraryTitleAsync(LibraryTitle? libraryTitle)
    {
        await Task.Run(() => libraryTitle is not null ? _titleLibraryRepository.Create(libraryTitle) : null);
    }

    public bool DropDbCollection(string collectionName)
    {
        return Db.DropCollection(collectionName);
    }

    public DateTime? GetRegionLastUpdate(string region, CancellationToken cancellationToken = default)
    {
        var regionTitleRepository = RegionRepository(region);
        var firstTitle = regionTitleRepository.FindOne(o => true);
        return firstTitle?.CreatedTime;
    }
    
    public ContentDistribution GetContentDistribution()
    {
        var library = _titleLibraryRepository.GetTitlesAsQueryable();
        var libraryList = library.ToList();
        var baseGames = libraryList.Where(o => o.Type == TitleLibraryType.Base);
        var updates = libraryList.Where(o => o.Type == TitleLibraryType.Update);
        var dlcs = libraryList.Where(o => o.Type == TitleLibraryType.DLC);
        var contentDistribution = new ContentDistribution
        {
                Base = baseGames.Count(),
                Updates = updates.Count(),
                Dlcs = dlcs.Count()
        };
        return contentDistribution;
    }

    public LibraryStats GetLibraryTitlesStats()
    {
        var library = _titleLibraryRepository.GetTitlesAsQueryable();
        var baseGames = library.Where(o => o.Type == TitleLibraryType.Base);
        var categories = new Dictionary<string, int>();
        foreach (var game in baseGames)
        {
            if (game.Category is null) continue;
            foreach (var category in game.Category)
            {
                if (categories.TryGetValue(category, out var value))
                {
                    categories[category] = value + 1;
                }
                else
                {
                    categories.Add(category, 1);
                }
            }
        }

        var sortedCategories = categories.OrderByDescending(o => o.Value);
        var stats = new LibraryStats
        {
                CategoriesGames = sortedCategories.ToDictionary(),
                ContentDistribution = GetContentDistribution()
        };
        return stats;
    }

    /*
    public async Task SaveDataGridStateAsync(string name, DataGridSettings settings)
    {
        await _settingsRepository.SaveDataGridStateAsync(name, settings);
    }

    public async Task<DataGridSettings?> LoadDataGridStateAsync(string name)
    {
        return await _settingsRepository.LoadDataGridStateAsync(name);
    }
*/
    public IEnumerable<GameVersions> GetTitleDbVersions(string titleTitleId)
    {
        return _titleDbVersionsRepository.FindByTitleId(titleTitleId);
    }

    public int ImportTitleDbVersions(List<GameVersions> gameVersions)
    {
        return _titleDbVersionsRepository.InsertBulk(gameVersions);
    }
    
    public int ImportTitleDbCnmts(List<PackagedContentMeta> packagedContentMeta)
    {
        return _titleDbCnmtsRepository.InsertBulk(packagedContentMeta);
    }
    
    public int ImportTitleDbRegionTitles(JObject titles, string region)
    {
        var regionTitleRepository = RegionRepository(region);
        var entities = new List<RegionTitle>();
        var currentDateTime = DateTime.Now;
        foreach (var title in titles)
        {
            if (title.Value is null || title.Value.ToString() == "{}") continue;
            var titleDbTitle = JsonConvert.DeserializeObject<TitleDbTitle>(title.Value.ToString());
            var regionTitle = _mapper.Map<RegionTitle>(titleDbTitle);
            regionTitle.Region = region;
            regionTitle.CreatedTime = currentDateTime;
            var cnmt = _titleDbCnmtsRepository.FindOne(x => x.TitleId == regionTitle.TitleId);
            if (cnmt != null)
            {
                regionTitle.Type = cnmt.TitleType switch
                {
                        128 => TitleLibraryType.Base,
                        129 => TitleLibraryType.Update,
                        130 => TitleLibraryType.DLC,
                        _ => TitleLibraryType.Unknown
                };
            }

            entities.Add(regionTitle);
            _logger.LogDebug("{Message}", regionTitle.Name);
        }
        return regionTitleRepository.InsertBulk(entities);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            _db?.Dispose();
        }

        _disposed = true;
    }

    ~DataService()
    {
        Dispose(false);
    }
}