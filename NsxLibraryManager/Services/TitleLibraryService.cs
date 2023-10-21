
using AutoMapper;
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
    private readonly IMapper _mapper;
    private readonly ILogger<TitleLibraryService> _logger;

    
    public TitleLibraryService(
            IDataService dataService, 
            IFileInfoService fileInfoService, 
            ITitleDbService titleDbService,
            IOptions<AppSettings> configuration,
            IMapper mapper,
            ILogger<TitleLibraryService> logger)
    {
        _dataService = dataService;
        _fileInfoService = fileInfoService;
        _configuration = configuration.Value;
        _logger = logger;
        _titleDbService = titleDbService;
        _mapper = mapper;
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
        
        libraryTitle.BannerUrl = regionTitle.BannerUrl;
        libraryTitle.Nsuid = regionTitle.Id;
        libraryTitle.NumberOfPlayers = regionTitle.NumberOfPlayers;
        libraryTitle.ReleaseDate = regionTitle.ReleaseDate;

        
        libraryTitle.Category = regionTitle.Category;
        libraryTitle.Developer = regionTitle.Developer;
        libraryTitle.Description = regionTitle.Description;
        libraryTitle.FrontBoxArt = regionTitle.FrontBoxArt;
        libraryTitle.Intro = regionTitle.Intro;
        libraryTitle.IconUrl = regionTitle.IconUrl;
        libraryTitle.Rating = regionTitle.Rating;
        libraryTitle.RatingContent = regionTitle.RatingContent;
        libraryTitle.Screenshots = regionTitle.Screenshots;
        libraryTitle.Size = regionTitle.Size;
        libraryTitle.Languages = regionTitle.Languages;
        libraryTitle.Screenshots = regionTitle.Screenshots;
        libraryTitle.RatingContent = regionTitle.RatingContent;

        var dlcVal = (int) TitleLibraryType.DLC;
        var dlc = (from cnmt in packagedContentMetas where cnmt.TitleType == dlcVal select cnmt.TitleId).ToList();

        if (dlc.Any())
        {
            libraryTitle.AvailableDlcs = dlc;
        }
        return libraryTitle;
    }
    
    public bool DropLibrary()
    {
        return _dataService.DropDbCollection(AppConstants.LibraryCollectionName);
    }

    public LibraryTitle? GetTitle(string titleId)
    {
        var libraryTitle = _dataService.GetLibraryTitleById(titleId);
        return libraryTitle;
    }

    public async Task<LibraryTitle?> GetTitleFromTitleDb(string titleId)
    {
        var regionTitle = await _titleDbService.GetTitle(titleId);
        if (regionTitle is not null)
        {
            var libraryTitle = _mapper.Map<LibraryTitle>(regionTitle);
            libraryTitle.FileName = string.Empty;
            return libraryTitle;
        }

        return null;
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

    //
    // Summary: This method should be called after the library has been refreshed
    //          to add any owned DLCs to the library, it must be done after all the tiles are in the db
    //          otherwise we would have to do a lot of lookups to see if the DLC is already in the library
    public async Task AddOwnedDlcToTitlesAsync()
    {
        var libraryTitles = await _dataService.GetLibraryTitlesQueryableAsync();
        var gamesWithDlc = libraryTitles.Where(x => x.AvailableDlcs != null && x.AvailableDlcs.Any());


        foreach (var dlcGame in gamesWithDlc)
        {
            if (dlcGame.AvailableDlcs == null) continue;
            var ownedDlc = new List<string>();
            foreach (var dlc in dlcGame.AvailableDlcs)
            {
                //var titleFound = libraryTitles.FirstOrDefault(x => x.TitleId == dlc);
                var titleFound = _dataService.GetLibraryTitleById(dlc);
                if (titleFound is null) continue;
                ownedDlc.Add(dlc);
            }

            if (!ownedDlc.Any()) continue;
            dlcGame.OwnedDlcs = ownedDlc;
            await _dataService.UpdateLibraryTitleAsync(dlcGame);
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