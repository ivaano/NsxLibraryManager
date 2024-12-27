using System.Diagnostics;
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
        //await _nsxLibraryDbContext.Database.ExecuteSqlAsync($"DELETE FROM Titles");

        await _nsxLibraryDbContext.Titles.ExecuteDeleteAsync();
        
        //await _nsxLibraryDbContext.Database.ExecuteSqlRawAsync($"DELETE FROM sqlite_sequence WHERE name = 'Titles'");
        //await _nsxLibraryDbContext.Database.ExecuteSqlRawAsync($"VACUUM");
        //await Task.Delay(1);
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
            var metaFromFileName = await _fileInfoService.GetFileInfoFromFileName(libraryTitle.FileName);

            title.TitleName = string.IsNullOrEmpty(libraryTitle.TitleName) 
                ? metaFromFileName.TitleName : libraryTitle.TitleName;

            //If no Type try use the filename
            if (libraryTitle.Type == TitleLibraryType.Unknown)
            {
                title.ContentType = metaFromFileName.Type switch
                {
                    TitleLibraryType.Base => TitleContentType.Base,
                    TitleLibraryType.Update => TitleContentType.Update,
                    TitleLibraryType.DLC => TitleContentType.DLC,
                    _ => TitleContentType.Unknown
                };
            }
            else
            {
                title.ContentType = libraryTitle.Type switch
                {
                    TitleLibraryType.Base => TitleContentType.Base,
                    TitleLibraryType.Update => TitleContentType.Update,
                    TitleLibraryType.DLC => TitleContentType.DLC,
                    _ => TitleContentType.Unknown
                };
            }
            
            title.BannerUrl = libraryTitle.BannerUrl;
            title.Description = libraryTitle.Description;
            title.Developer = libraryTitle.Developer;
            title.IconUrl = libraryTitle.IconUrl;
            title.Intro = libraryTitle.Intro;
            title.LastWriteTime = libraryTitle.LastWriteTime;
            title.NsuId = libraryTitle.Nsuid;
            title.NumberOfPlayers = libraryTitle.NumberOfPlayers;
            title.PackageType = metaFromFileName.PackageType;
            title.Publisher = libraryTitle.Publisher;
            title.Rating = libraryTitle.Rating;
            title.ReleaseDate = libraryTitle.ReleaseDate;
            title.Size = metaFromFileName.Size;
            title.Version = (int?)libraryTitle.TitleVersion;

            
            /*            
            if (libraryTitle.Type == TitleLibraryType.Base)
            {
                title.IconUrl = await _fileInfoService.GetFileIcon(libraryTitle.FileName);    
            }
            */
            return title;
        }

        title.BannerUrl = titledbTitle.BannerUrl;
        title.ContentType = titledbTitle.ContentType;
        title.Description = titledbTitle.Description;
        title.Developer = titledbTitle.Developer;
        title.DlcCount = titledbTitle.DlcCount;
        title.DlcCount = titledbTitle.DlcCount;
        title.IconUrl = titledbTitle.IconUrl;
        title.Intro = titledbTitle.Intro;
        title.LastWriteTime = libraryTitle.LastWriteTime;
        title.LatestVersion = titledbTitle.LatestVersion;
        title.NsuId = titledbTitle.NsuId;
        title.NumberOfPlayers = titledbTitle.NumberOfPlayers;
        title.OtherApplicationId = titledbTitle.OtherApplicationId;
        title.PackageType = libraryTitle.PackageType;
        // prefer the publisher from the file
        title.Publisher = titledbTitle.Publisher.ConvertNullOrEmptyTo(libraryTitle.Publisher);
        title.Rating = titledbTitle.Rating;
        title.Region = titledbTitle.Region;
        title.ReleaseDate = titledbTitle.ReleaseDate;
        title.Size = titledbTitle.Size;
        // prefer the title name from titledb
        title.TitleName = libraryTitle.TitleName.ConvertNullOrEmptyTo(titledbTitle.TitleName);
        title.UpdatesCount = titledbTitle.UpdatesCount;
        title.Version = (int?)libraryTitle.TitleVersion;
       
        return title; 
    }

    public async Task<bool> SaveDatabaseChangesAsync()
    {
        await _nsxLibraryDbContext.SaveChangesAsync();
        return true;
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
            //await _nsxLibraryDbContext.SaveChangesAsync();
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