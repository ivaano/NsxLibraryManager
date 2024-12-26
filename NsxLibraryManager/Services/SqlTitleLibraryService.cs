using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Extensions;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Core.Settings;
using NsxLibraryManager.Data;
using NsxLibraryManager.Models.NsxLibrary;
using NsxLibraryManager.Services.Interface;

namespace NsxLibraryManager.Services;

public class SqlTitleLibraryService : ISqlTitleLibraryService
{
    private readonly NsxLibraryDbContext _nsxLibraryDbContext;
    private readonly TitledbDbContext _titledbDbContext;
    private readonly IFileInfoService _fileInfoService;
    private readonly AppSettings _configuration;
    private readonly ILogger<SqlTitleLibraryService> _logger;

    
    public SqlTitleLibraryService(
        NsxLibraryDbContext nsxLibraryDbContext,
        TitledbDbContext titledbDbContext,
        IFileInfoService fileInfoService,
        IOptionsMonitor<AppSettings> configuration,
        ILogger<SqlTitleLibraryService> logger)
    {
        _nsxLibraryDbContext = nsxLibraryDbContext ?? throw new ArgumentNullException(nameof(nsxLibraryDbContext));
        _titledbDbContext = titledbDbContext ?? throw new ArgumentNullException(nameof(titledbDbContext));
        _fileInfoService = fileInfoService;
        _configuration = configuration.CurrentValue;
        _logger = logger;


    }
    public async Task<bool> DropLibrary()
    {
        /*
        await _nsxLibraryDbContext.Database.ExecuteSqlAsync($"DELETE FROM TitleRegion");
        await _nsxLibraryDbContext.Database.ExecuteSqlAsync($"DELETE FROM TitleCategory");
        await _nsxLibraryDbContext.Database.ExecuteSqlAsync($"DELETE FROM TitleLanguages");
        await _nsxLibraryDbContext.Database.ExecuteSqlAsync($"DELETE FROM TitleRatingContents");
        await _nsxLibraryDbContext.Database.ExecuteSqlAsync($"DELETE FROM Cnmts");
        await _nsxLibraryDbContext.Database.ExecuteSqlAsync($"DELETE FROM Editions");
        await _nsxLibraryDbContext.Database.ExecuteSqlAsync($"DELETE FROM Versions");
        await _nsxLibraryDbContext.Database.ExecuteSqlAsync($"DELETE FROM ScreenShots");
        await _nsxLibraryDbContext.Database.ExecuteSqlAsync($"DELETE FROM sqlite_sequence WHERE name = 'Cnmts'");
        await _nsxLibraryDbContext.Database.ExecuteSqlAsync($"DELETE FROM sqlite_sequence WHERE name = 'Edition'");
        await _nsxLibraryDbContext.Database.ExecuteSqlAsync($"DELETE FROM sqlite_sequence WHERE name = 'Versions'");
        await _nsxLibraryDbContext.Database.ExecuteSqlAsync($"DELETE FROM sqlite_sequence WHERE name = 'ScreenShots'");
        */
        await _nsxLibraryDbContext.Database.ExecuteSqlAsync($"DELETE FROM Titles");
        await _nsxLibraryDbContext.Database.ExecuteSqlAsync($"DELETE FROM sqlite_sequence WHERE name = 'Titles'");
        //return _nsxLibraryDbContext.Database.EnsureDeleted();
        return true;
    }

    public async Task<IEnumerable<string>> GetFilesAsync()
    {
        var files = await _fileInfoService.GetFileNames(_configuration.LibraryPath,
            _configuration.Recursive);
        return files;
    }

    private async Task<Title> AggregateLibraryTitle(LibraryTitle libraryTitle)
    {
        var titledbTitle = await _titledbDbContext.Titles
            .FirstOrDefaultAsync(t => t.ApplicationId == libraryTitle.TitleId);
        
        var title = new Title
        {
            FileName = libraryTitle.FileName,
            ApplicationId = libraryTitle.TitleId,
        };
        
        if (titledbTitle is null)
        {
            if (libraryTitle.Type == TitleLibraryType.Base)
            {
                title.IconUrl = await _fileInfoService.GetFileIcon(libraryTitle.FileName);    
            }
            
            title.Size = await _fileInfoService.GetFileSize(libraryTitle.FileName);
            return title;
        }
        // prefer the title name from the file
        title.TitleName = libraryTitle.TitleName.ConvertNullOrEmptyTo(titledbTitle.TitleName);
        // prefer the publisher from the titledb
        title.Publisher = titledbTitle.Publisher.ConvertNullOrEmptyTo(libraryTitle.Publisher);
        
        title.BannerUrl = titledbTitle.BannerUrl;
        title.NsuId = titledbTitle.NsuId;
        title.NumberOfPlayers = titledbTitle.NumberOfPlayers;
        title.ReleaseDate = titledbTitle.ReleaseDate;
        title.Developer = titledbTitle.Developer;
        title.Description = titledbTitle.Description;
        title.Intro = titledbTitle.Intro;
        title.IconUrl = titledbTitle.IconUrl;
        title.Rating = titledbTitle.Rating;
        title.Size = titledbTitle.Size;
        title.DlcCount = titledbTitle.DlcCount;
        title.UpdatesCount = titledbTitle.UpdatesCount;
        title.LastWriteTime = libraryTitle.LastWriteTime;
        
        return title; 
    }

    public async Task<LibraryTitle?> ProcessFileAsync(string file)
    {
        try
        {
            _logger.LogDebug("Processing file: {file}", file);
            var libraryTitle = await _fileInfoService.GetFileInfo(file, detailed: false);
            if (libraryTitle is null)
            {
                _logger.LogDebug("Unable to get File Information from file : {file}", file);
                return libraryTitle;
            }
            var title = await AggregateLibraryTitle(libraryTitle);
            _nsxLibraryDbContext.Add(title);
            await _nsxLibraryDbContext.SaveChangesAsync();
            var caco = true;
            /*
            var titledbTitle = await _titleDbService.GetTitle(libraryTitle.TitleId);
            var titleDbCnmt = _titleDbService.GetTitleCnmts(libraryTitle.TitleId);
            libraryTitle = await AggregateLibraryTitle(libraryTitle, titledbTitle, titleDbCnmt);
            await _dataService.AddLibraryTitleAsync(libraryTitle);
            */
            return libraryTitle;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error processing file: {file}", file);
            return null;
        }
    }
}