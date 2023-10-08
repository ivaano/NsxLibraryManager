using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
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

    public IEnumerable<string> GetRegionsToImport()
    {
        return _configuration.DownloadSettings.Regions;
    }
}