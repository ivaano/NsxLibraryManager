using Microsoft.Extensions.Options;
using NsxLibraryManager.Enums;
using NsxLibraryManager.Extensions;
using NsxLibraryManager.Models;
using NsxLibraryManager.Settings;

namespace NsxLibraryManager.Services;

public class TitleLibraryService : ITitleLibraryService
{
    private readonly IDataService _dataService;
    private readonly IFileInfoService _fileInfoService;
    private readonly ITitleDbService _titleDbService;
    private readonly AppSettings _configuration;
    private readonly ILogger<TitleLibraryService> _logger;

    
    public TitleLibraryService(
            IDataService dataService, 
            IFileInfoService fileInfoService, 
            ITitleDbService titleDbService,
            IOptions<AppSettings> configuration,
            ILogger<TitleLibraryService> logger)
    {
        _dataService = dataService;
        _fileInfoService = fileInfoService;
        _configuration = configuration.Value;
        _logger = logger;
        _titleDbService = titleDbService;
    }


    private LibraryTitle AggregateLibraryTitle(LibraryTitle libraryTitle, RegionTitle? regionTitle, IEnumerable<PackagedContentMeta> packagedContentMetas)
    {
        if (regionTitle is null) return libraryTitle;
        
        if (libraryTitle.Type != TitleLibraryType.Base)
        {
            libraryTitle.ApplicationTitleName = regionTitle.Name;
        }
        // prefer the title name from the file
        libraryTitle.TitleName = libraryTitle.TitleName.ConvertNullOrEmptyTo(regionTitle.Name);
        libraryTitle.Publisher = libraryTitle.Publisher.ConvertNullOrEmptyTo(regionTitle.Publisher);

        libraryTitle.Category = regionTitle.Category;
        libraryTitle.Developer = regionTitle.Developer;
        libraryTitle.Description = libraryTitle.Description;
        libraryTitle.FrontBoxArt = regionTitle.FrontBoxArt;
        libraryTitle.Intro = regionTitle.Intro;
        libraryTitle.BannerUrl = regionTitle.BannerUrl;
        libraryTitle.IconUrl = regionTitle.IconUrl;
        libraryTitle.Screenshots = regionTitle.Screenshots;

        var dlcVal = TitleLibraryType.DLC;
        //var intDlc = BitConverter.ToInt32(dlcVal);   //Convert.ToUInt32(TitleLibraryType.DLC.ToString(), 16);
        var dlc = (from cnmt in packagedContentMetas where cnmt.TitleType == 130 select cnmt.TitleId).ToList();

        if (dlc.Any())
        {
            libraryTitle.Dlcs = dlc;
        }
        return libraryTitle;
    }
    
    public bool DropLibrary()
    {
        return _dataService.DropDbCollection(AppConstants.LibraryCollectionName);
    }

    public async Task<bool> ProcessFileAsync(string file)
    {
        try
        {
            var libraryTitle = await _fileInfoService.GetFileInfo(file);
            var titledbTitle = await _titleDbService.GetTitle(libraryTitle.TitleId);
            var titleDbCnmt = await _titleDbService.GetTitleCnmts(libraryTitle.TitleId);
            libraryTitle = AggregateLibraryTitle(libraryTitle, titledbTitle, titleDbCnmt);
            await _dataService.AddLibraryTitleAsync(libraryTitle);
            
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error processing file: {file}");
            return false;
        }
    }

    public string GetLibraryPath()
    {
        return Path.GetFullPath(_configuration.LibraryPath);
    }

    public async Task<IEnumerable<string>> GetFilesAsync()
    {
        var files = await _fileInfoService.GetFileNames(_configuration.LibraryPath,
                                                                        _configuration.Recursive);
        return files;
    }
}