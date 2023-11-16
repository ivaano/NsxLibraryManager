using System.Globalization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NsxLibraryManager.Enums;
using NsxLibraryManager.Models;
using NsxLibraryManager.Models.Dto;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Settings;
using NsxLibraryManager.Utils;

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
                        VersionShifted = versions.Key.VersionShifted(),
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

    public IEnumerable<GameVersions> GetVersions(string titleTitleId)
    {
        var patchId = GetTitleCnmts(titleTitleId);
        var versions = _dataService.GetTitleDbVersions(titleTitleId);
        var patchFound = patchId.FirstOrDefault(x => x.TitleType == 129);
        var versionList = versions.ToList();
        if (patchFound == null) return versionList;
        foreach (var version in versionList)
        {
            var tryParseDate = DateTime.TryParseExact(version.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var parsedDate)
                    ? parsedDate
                    : new DateTime();
            version.Date = parsedDate.ToString("MM/dd/yyyy");
            version.ApplicationId = titleTitleId.ToUpper();
            version.TitleId = patchFound.TitleId.ToUpper();
        }
        return  versionList;
    }

    public Task<uint> GetAvailableVersion(string titleTitleId)
    {
        var versions = _dataService.GetTitleDbVersions(titleTitleId);
        var latestVersion = versions.MaxBy(o => o.Date);
        var algo = Convert.ToUInt32(latestVersion?.Version);
        return Task.FromResult(algo);
    }

    // Search in all regions for the title
    public async Task<RegionTitle?> GetTitle(string titleTitleId)
    {
        var regions = _configuration.DownloadSettings.Regions;
        foreach (var region in regions)
        {
            var regionTitle = await _dataService.GetTitleDbRegionTitleByIdAsync(region, titleTitleId);
            if (regionTitle != null)
            {
                return regionTitle;
            }
        }
        return null;
    }
    
    public async Task<IEnumerable<Dlc>> GetTitleDlc(string titleTitleId)
    {
        
        var packagedContentMetas = GetTitleCnmts(titleTitleId);
        var dlcVal = (int) TitleLibraryType.DLC;
        var cnmts = packagedContentMetas
                .OrderByDescending(p => p.Version)
                .Where(p => p.TitleType == dlcVal);

        var dlcList = new List<Dlc>();        
        foreach (var cnmt in cnmts)
        {
            var regionTitle = await GetTitle(cnmt.TitleId);
            if (regionTitle != null)
            {
                var updateDlc = false;
                var alreadyInList = false;
                foreach (var exists in dlcList)
                {
                    if (exists.TitleId == cnmt.TitleId)
                    {
                        alreadyInList = true;
                        var existsVersion = Convert.ToUInt32(exists.TitleVersion);
                        var currentVersion = Convert.ToUInt32(cnmt.Version);
                        
                        if (currentVersion > existsVersion)
                        {
                            exists.TitleVersion = cnmt.Version;
                            updateDlc = true;
                        }
                    }
                }

                if (!alreadyInList)
                {
                    var dlc = new Dlc
                    {
                            TitleId = regionTitle.TitleId,
                            TitleVersion = cnmt.Version,
                            TitleName = regionTitle.Name
                    };
                    if (!updateDlc)
                    {
                        dlcList.Add(dlc);    
                    }      
                }
            }
        }
        return dlcList;
    }

    public IEnumerable<PackagedContentMeta> GetTitleCnmts(string titleTitleId)
    {
        return _dataService.GetTitleDbCnmtsForTitle(titleTitleId);
    }
}