using System.Globalization;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Extensions;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Core.Settings;

namespace NsxLibraryManager.Core.Services;

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


    private LibraryTitle? AggregateLibraryTitle(LibraryTitle? libraryTitle, RegionTitle? regionTitle,
            IEnumerable<PackagedContentMeta> packagedContentMetas)
    {
        if (regionTitle is null) return libraryTitle;
        if (libraryTitle is null) return libraryTitle;

        if (libraryTitle.Type != TitleLibraryType.Base)
        {
            libraryTitle.ApplicationTitleName = regionTitle.Name;
        }

        // prefer the title name from the file
        libraryTitle.TitleName = libraryTitle.TitleName.ConvertNullOrEmptyTo(regionTitle.Name);
        // prefer the publisher from the titledb
        libraryTitle.Publisher = regionTitle.Publisher.ConvertNullOrEmptyTo(libraryTitle.Publisher);

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
        libraryTitle.LastUpdated = DateTime.Now;
        var dlcVal = (int)TitleLibraryType.DLC;
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

    public async Task<LibraryTitle?> ProcessFileAsync(string file)
    {
        try
        {
            var libraryTitle = await _fileInfoService.GetFileInfo(file);
            if (libraryTitle is null) return libraryTitle;
            var titledbTitle = await _titleDbService.GetTitle(libraryTitle.TitleId);
            var titleDbCnmt = _titleDbService.GetTitleCnmts(libraryTitle.TitleId);
            libraryTitle = AggregateLibraryTitle(libraryTitle, titledbTitle, titleDbCnmt);
            await _dataService.AddLibraryTitleAsync(libraryTitle);
            return libraryTitle;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error processing file: {file}", file);
            return null;
        }
    }

    public bool AddOwnedUpdatesToTitle(LibraryTitle title)
    {
        var versions = _dataService.GetTitleDbVersions(title.TitleId);
        var ownedVersions = new List<int>();
        uint lastOwnedVersion = 0;
        foreach (var version in versions)
        {
            var libraryTitles = _dataService.GetLibraryTitlesQueryableAsync();
            var patchTitle = libraryTitles.
                    Where(x => x.ApplicationTitleId == version.TitleId).Where(x => x.PatchNumber == version.VersionShifted).
                    Where(x => x.Type == TitleLibraryType.Update).
                    FirstOrDefault();
            if (patchTitle is null) continue;
            _ = DateTime.TryParseExact((string?)version.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var parsedDate)
                    ? parsedDate
                    : new DateTime();
            patchTitle.ReleaseDate = parsedDate;
            var patchVersion = Convert.ToUInt32(version.VersionShifted);
            if (lastOwnedVersion < patchVersion)
            {
                lastOwnedVersion = patchVersion;
            }
            _dataService.UpdateLibraryTitleAsync(patchTitle);
            ownedVersions.Add(version.VersionShifted);
        }
        title.OwnedUpdates = ownedVersions;
        title.LastOwnedVersion = lastOwnedVersion;
        _dataService.UpdateLibraryTitleAsync(title);
        return true;
    }

    public bool AddOwnedDlcsToTitle(LibraryTitle title)
    {
        if (title.AvailableDlcs == null) return true;
        var ownedDlc = new List<string>();
        foreach (var dlc in title.AvailableDlcs)
        {
            //var titleFound = libraryTitles.FirstOrDefault(x => x.TitleId == dlc);
            var titleFound = _dataService.GetLibraryTitleById(dlc);
            if (titleFound is null) continue;
            ownedDlc.Add(dlc);
        }

        if (!ownedDlc.Any()) return true;
        title.OwnedDlcs = ownedDlc;
        _dataService.UpdateLibraryTitleAsync(title);
        return true;
    }
    
    public Task<string> ProcessTitleDlcs(LibraryTitle title)
    {
        var updatedTitleId = string.Empty;
        if (title.Type == TitleLibraryType.Base)
        {
            AddOwnedDlcsToTitle(title);
            updatedTitleId = title.TitleId;
        }
        else
        {
            var libraryTitles = _dataService.GetLibraryTitlesQueryableAsync();
            var baseTitle = libraryTitles.
                    Where(x => x.TitleId == title.ApplicationTitleId).
                    Where(x => x.Type == TitleLibraryType.Base).
                    FirstOrDefault();
            if (baseTitle is null) return Task.FromResult(string.Empty);
            AddOwnedDlcsToTitle(baseTitle);
            updatedTitleId = baseTitle.TitleId;
        }
        return Task.FromResult(updatedTitleId);
    }
    
    public Task<string> ProcessTitleUpdates(LibraryTitle title)
    {
        var updatedTitleId = string.Empty;
        if (title.Type == TitleLibraryType.Base)
        {
            AddOwnedUpdatesToTitle(title);
            updatedTitleId = title.TitleId;
        }
        else
        {
            var libraryTitles = _dataService.GetLibraryTitlesQueryableAsync();
            var baseTitle = libraryTitles.
                    Where(x => x.TitleId == title.ApplicationTitleId).
                    Where(x => x.Type == TitleLibraryType.Base).
                    FirstOrDefault();
            if (baseTitle is null) return Task.FromResult(string.Empty);
            AddOwnedUpdatesToTitle(baseTitle);
            updatedTitleId = baseTitle.TitleId;
        }
        return Task.FromResult(updatedTitleId);
    }
    
    public Task ProcessAllTitlesUpdates()
    {
        var libraryTitles = _dataService.GetLibraryTitlesQueryableAsync();
        var gamesWithUpdates = libraryTitles.Where(x => x.AvailableVersion > 0);

        foreach (var title in gamesWithUpdates)
        {
            AddOwnedUpdatesToTitle(title);
        }

        return Task.CompletedTask;
    }


    //
    // Summary: This method should be called after the library has been reloaded
    //          to add any owned DLCs to the library, it must be done after all the tiles are in the db
    //          otherwise we would have to do a lot of lookups to see if the DLC is already in the library
    public Task ProcessAllTitlesDlc()
    {
        var libraryTitles = _dataService.GetLibraryTitlesQueryableAsync();
        var gamesWithDlc = libraryTitles.Where(x => x.AvailableDlcs != null && x.AvailableDlcs.Any());

        foreach (var dlcGame in gamesWithDlc)
        {
            AddOwnedDlcsToTitle(dlcGame);
        }

        return Task.CompletedTask;
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
    
    public Task<LibraryTitle> DeleteTitleAsync(string titleId)
    {
        var title = _dataService.GetLibraryTitleById(titleId);
        if (title is null) return Task.FromResult(new LibraryTitle
        {
                FileName = string.Empty,
                TitleId = titleId
        });
        var result = _dataService.DeleteLibraryTitle(titleId);
        if (title.Type != TitleLibraryType.Update) return Task.FromResult(title);
        var baseTitle = _dataService.GetLibraryTitleById(title.ApplicationTitleId ?? string.Empty);
        return Task.FromResult(baseTitle ?? title);
    }

    public async Task<(IEnumerable<string> filesToAdd,  IEnumerable<string> titlesToRemove)> GetDeltaFilesInLibraryAsync()
    {
        var files = await GetFilesAsync();
        var pathFileList = files.ToList();
        var libraryTitles = await _dataService.GetLibraryTitlesAsync();
        var libraryList = libraryTitles.ToList();
        var libraryTitlesDict = libraryList.ToDictionary(x => x.FileName, x => x.TitleId);
        var libraryFiles = libraryList.Select(x => x.FileName).ToList(); 
        var filesToAdd = pathFileList.Except(libraryFiles);
        var filesToRemove = libraryFiles.Except(pathFileList);
        var titlesToRemove = filesToRemove.Select(fileName => libraryTitlesDict[fileName]).ToList();
        return (filesToAdd, titlesToRemove);
    }
}