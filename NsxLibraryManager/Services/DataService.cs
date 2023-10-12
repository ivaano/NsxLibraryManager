using System.Globalization;
using AutoMapper;
using LiteDB;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NsxLibraryManager.Models;
using NsxLibraryManager.Repository;
using NsxLibraryManager.Settings;

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
    private readonly IMapper _mapper;
    private readonly ILogger<DataService> _logger;
    
    private ILiteDatabase Db
    {
        get { return _db ??= new LiteDatabase(_connStr); }
    }

    public DataService(IOptions<AppSettings> configuration, IMapper mapper, ILogger<DataService> logger)
    {
        _logger = logger;
        _mapper = mapper;
        var titleDbLocation = configuration.Value.TitleDatabase ?? throw new InvalidOperationException();
        _connStr = Path.Combine(titleDbLocation);
        _titleDbCnmtsRepository = new TitleDbCnmtsRepository(Db);
        _titleDbVersionsRepository = new TitleDbVersionsRepository(Db);
        _titleLibraryRepository = new TitleLibraryRepository(Db);
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
    
    public ITitleLibraryRepository TitleLibraryRepository()
    {
        return _titleLibraryRepository ??= new TitleLibraryRepository(Db);
    }

    public ITitleDbCnmtsRepository TitleDbCnmtsRepository()
    {
        return _titleDbCnmtsRepository ??= new TitleDbCnmtsRepository(Db);
    }
    
    public ITitleDbVersionsRepository TitleDbVersionsRepositoryRepository()
    {
        return _titleDbVersionsRepository ??= new TitleDbVersionsRepository(Db);
    }

    public async Task<IEnumerable<RegionTitle>> GetTitleDbRegionTitlesAsync(string region)
    {
        var regionTitleRepository = RegionRepository(region);
        return await Task.Run(() => regionTitleRepository.All());
    }

    public async Task<RegionTitle?> GetTitleDbRegionTitleByIdAsync(string region, string titleId)
    {
        var regionTitleRepository = RegionRepository(region);
        var regionTitle = regionTitleRepository.FindOne(x => x.TitleId == titleId) ?? regionTitleRepository.FindTitleByIds(titleId);
        return await Task.Run(() => regionTitle);
    }

    public async Task<IEnumerable<PackagedContentMeta>> GetTitleDbCnmtsForTitleAsync(string titleId)
    {
        return await Task.Run(() => _titleDbCnmtsRepository.FindByOtherApplicationIdId(titleId));
    }

    public async Task<IEnumerable<LibraryTitle>> GetLibraryTitlesAsync()
    {
        return await Task.Run(() => _titleLibraryRepository.All());
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

    public List<GameVersions> GetTitleDbVersions(string titleTitleId)
    {
        return _titleDbVersionsRepository.FindByTitleId(titleTitleId).ToList();
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
            if (DateTime.TryParseExact(regionTitle.ReleaseDate.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None,  out var parsedDate))
            {
                regionTitle.ReleaseDateOnly = parsedDate;
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