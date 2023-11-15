using System.Globalization;
using AutoMapper;
using LiteDB;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NsxLibraryManager.Enums;
using NsxLibraryManager.Models;
using NsxLibraryManager.Repository;
using NsxLibraryManager.Repository.Interface;
using NsxLibraryManager.Settings;
using Radzen;

namespace NsxLibraryManager.Services;

public class DataService : IDataService
{
    private readonly string _connStr;
    private ILiteDatabase? _db;
    private bool _disposed;
    private ITitleRepository? _titleRepository;
    private Dictionary<string, IRegionRepository>? _regionRepository;
    private ITitleLibraryRepository _titleLibraryRepository;
    private ITitleDbCnmtsRepository _titleDbCnmtsRepository;
    private ITitleDbVersionsRepository _titleDbVersionsRepository;
    private ISettingsRepository _settingsRepository;
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
        _settingsRepository = new SettingsRepository(Db);
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
//        var regionTitleRepository = RegionRepository(region);
//        return await Task.Run(() => regionTitleRepository.All());
    }

    public async Task<RegionTitle?> GetTitleDbRegionTitleByIdAsync(string region, string titleId)
    {
        var regionTitleRepository = RegionRepository(region);
        var regionTitle = regionTitleRepository.FindOne(x => x.TitleId == titleId) ?? regionTitleRepository.FindTitleByIds(titleId);
        return await Task.Run(() => regionTitle);
    }

    public async Task<IEnumerable<PackagedContentMeta>> GetTitleDbCnmtsForTitleAsync(string titleId)
    {
        return await Task.Run(() => _titleDbCnmtsRepository.FindByOtherApplicationId(titleId));
    }

    public async Task<IEnumerable<LibraryTitle>> GetLibraryTitlesAsync()
    {
        return await Task.Run(() => _titleLibraryRepository.All());
    }

    public async Task<IQueryable<LibraryTitle>> GetLibraryTitlesQueryableAsync()
    {
        return await Task.Run(() => _titleLibraryRepository.GetTitlesAsQueryable());
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

    public async Task AddLibraryTitleAsync(LibraryTitle libraryTitle)
    {
        await Task.Run(() => _titleLibraryRepository.Create(libraryTitle));
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

    public async Task SaveDataGridStateAsync(string name, DataGridSettings settings)
    {
        await _settingsRepository.SaveDataGridStateAsync(name, settings);
    }

    public async Task<DataGridSettings?> LoadDataGridStateAsync(string name)
    {
        return await _settingsRepository.LoadDataGridStateAsync(name);
    }

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
            if (title.Value != null && title.Value.ToString() == "{}") continue;

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