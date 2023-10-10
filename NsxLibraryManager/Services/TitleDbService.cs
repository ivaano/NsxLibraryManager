using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NsxLibraryManager.Models;
using NsxLibraryManager.Settings;

namespace NsxLibraryManager.Services;

public class TitleDbService : ITitleDbService
{
    private readonly AppSettings _configuration;
    private readonly IDownloadService _downloadService;
    private readonly IDataService _dataService;
    
    public TitleDbService(
            IOptions<AppSettings> configuration, 
            IDownloadService downloadService,
            IDataService dataService)
    {
        _configuration = configuration.Value;
        _downloadService = downloadService;
        _dataService = dataService;
    }
    
    public async Task ImportRegionAsync(string region)
    {
        var destFilePath = await _downloadService.GetRegionFile(region, CancellationToken.None);
        var titles = JObject.Parse(await File.ReadAllTextAsync(destFilePath));
        _dataService.DropDbCollection(region);
        _dataService.ImportTitleDbRegionTitles(titles, region);
    }

    public async Task ImportCnmtsAsync()
    {
        var destFilePath = await _downloadService.GetCnmtsFile(CancellationToken.None);
        var json = await File.ReadAllTextAsync(@destFilePath);
        var packagedContentMeta = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, PackagedContentMeta>>>(json);
        //flat the data
        var cnmts = new List<PackagedContentMeta>();
        foreach (var cnmt in packagedContentMeta)
        {
            var gameVersions = cnmt.Value;
            foreach (var gameVersion in gameVersions)
            {
                cnmts.Add(gameVersion.Value);
            }
        }
        _dataService.DropDbCollection(AppConstants.CnmtsCollectionName);
        _dataService.ImportTitleDbCnmts(cnmts);
    }

    public async Task ImportVersionsAsync()
    {
        var destFilePath = await _downloadService.GetVersionsFile(CancellationToken.None);
        var json = await File.ReadAllTextAsync(@destFilePath);
        var gameVersions  = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);
        var gameVersionsList = new List<GameVersions>();

        foreach (var game in gameVersions)
        {
            foreach (var versions in game.Value)
            {
                var gameVersion = new GameVersions
                {
                        TitleId = game.Key,
                        Version = versions.Key,
                        Date = versions.Value
                };
                gameVersionsList.Add(gameVersion);
            }
        }
        _dataService.DropDbCollection(AppConstants.VersionsCollectionName);
        _dataService.ImportTitleDbVersions(gameVersionsList);
    }

    public IEnumerable<string> GetRegionsToImport()
    {
        return _configuration.DownloadSettings.Regions;
    }
}